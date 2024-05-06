using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Assets.Scripts.SaveGameDatas.Attributes
{
    internal static class SerializationController
    {
        public static object GetSavedValue(this object serializationObject)
        {
            var type = serializationObject.GetType();
            if (!type.IsSerialization(out SerializationAttribute serialization)) return null;

            return serializationObject.GetValueOfSerialization(type, serialization.SerializedType);
        }

        public static void SetSavedValue(this object serializationObject, object serializedObject) =>
            serializedObject.SetValueOfSerialization(serializationObject);

        private static object GetValueOfSerialization(this object serializationObject, Type serializationType, Type serializedType)
        {
            if (serializedType == null) serializedType = serializationType;

            var serializedObject = Activator.CreateInstance(serializedType);

            if (serializedObject == null || !serializedType.TryGetMembersInfo(MemberTypes.Field | MemberTypes.Property, out MemberInfo[] members)) return false;
            var serializedMembers = members.Where(member => member.IsSerialization<SerializedMemberAttribute>(out _)).ToDictionary(member => member, member => { member.IsSerialization(out SerializedMemberAttribute attr); return attr; });

            foreach (var memberInfo in serializedMembers)
            {
                var value = memberInfo.Key.GetFieldValue(memberInfo.Value.SerializedMemberName, serializationObject, serializationType);

                if (memberInfo.Key is FieldInfo field && field.FieldType.IsAssignableFrom(value.GetType()) ||
                    memberInfo.Key is PropertyInfo property && property.PropertyType.IsAssignableFrom(value.GetType()))
                    memberInfo.Key.SetValue(serializedObject, value);
            }

            return serializedObject;
        }

        private static object GetFieldValue(this MemberInfo serializedField, string serializationMemberName, object serializationObject, Type serializationType)
        {
            serializationMemberName = string.IsNullOrEmpty(serializationMemberName) ? serializedField.Name : serializationMemberName;
            if (!serializationType.TryGetMemberInfo(serializationMemberName, out MemberInfo serializationMember)) return null;

            object serializationMemberValue = null;
            Type serializationMemberType = null;

            if (serializationMember is FieldInfo serializationField)
            {
                serializationMemberValue = serializationField.GetValue(serializationObject);
                serializationMemberType = serializationMemberValue != null ? serializationMemberValue.GetType() : serializationField.FieldType;
            }
            else if (serializationMember is PropertyInfo serializationProperty)
            {
                serializationMemberValue = serializationProperty.GetValue(serializationObject);
                serializationMemberType = serializationMemberValue != null ? serializationMemberValue.GetType() : serializationProperty.PropertyType;
            }

            if (serializationMemberType.TryGetValueOfDictionary(serializationMemberValue, out IDictionary serializationDictionary)) return serializationDictionary;
            else if (serializationMemberType.TryGetValueOfArray(serializationMemberValue, out Array serializationArray)) return serializationArray;
            else if (serializationMemberType.TryGetValueOfCollection(serializationMemberValue, out IList serializationCollection)) return serializationCollection;
            else if (serializationMemberType.TryGetValue(serializationMemberValue, out object value)) return value;

            return serializationMemberValue;
        }

        private static bool TryGetValue(this Type serializationMemberType, object serializationMemberValue, out object serializationValue)
        {
            serializationValue = null;
            if (!serializationMemberType.IsSerialization(out SerializationAttribute serialization)) return false;

            serializationValue = serializationMemberValue.GetValueOfSerialization(serializationMemberType, serialization.SerializedType);
            return true;
        }

        private static bool TryGetValueOfDictionary(this Type serializationMemberType, object serializationMemberValue, out IDictionary serializationDictionary)
        {
            serializationDictionary = null;

            if (!(serializationMemberValue is IDictionary)) return false;

            var dictionary = (IDictionary)serializationMemberValue;

            var keyType = serializationMemberType.GenericTypeArguments[0];
            var valueType = serializationMemberType.GenericTypeArguments[1];

            var keyIsSerialization = keyType.IsSerialization(out SerializationAttribute serializationAttr1);
            var valueIsSerialization = valueType.IsSerialization(out SerializationAttribute serializationAttr2);

            if (!keyIsSerialization && !valueIsSerialization)
            {
                serializationDictionary = dictionary;
                return false;
            }

            var newKeyType = keyType;
            var newValueType = valueType;
            if (keyIsSerialization) newKeyType = serializationAttr1.SerializedType;
            if (valueIsSerialization) newValueType = serializationAttr2.SerializedType;

            serializationDictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(newKeyType ?? keyType, newValueType ?? valueType));

            if (newKeyType == keyType && newValueType != valueType)
                foreach (DictionaryEntry keyValuePair in dictionary)
                    serializationDictionary.Add(keyValuePair.Key, keyValuePair.Value.GetValueOfSerialization(valueType, newValueType));

            else if (newValueType == valueType && newKeyType != newValueType)
                foreach (DictionaryEntry keyValuePair in dictionary)
                    serializationDictionary.Add(keyValuePair.Key.GetValueOfSerialization(keyType, newKeyType), keyValuePair.Value);

            else if (newKeyType != newValueType && newValueType != valueType)
                foreach (DictionaryEntry keyValuePair in dictionary)
                    serializationDictionary.Add(keyValuePair.Key.GetValueOfSerialization(keyType, newKeyType), keyValuePair.Value.GetValueOfSerialization(valueType, newValueType));

            else serializationDictionary = dictionary;

            return true;
        }

        private static bool TryGetValueOfArray(this Type memberType, object memberValue, out Array serializationArray)
        {
            serializationArray = null;

            Type elementType;
            if (!memberType.IsArray || !(elementType = memberType.GetElementType()).IsSerialization(out SerializationAttribute serializationAttr)) return false;

            var arrayValue = (Array)memberValue;
            var serializationType = serializationAttr.SerializedType ?? elementType;

            serializationArray = Array.CreateInstance(serializationType, arrayValue.Length);

            for (int i = 0; i < arrayValue.Length; i++)
                serializationArray.SetValue(arrayValue.GetValue(i).GetValueOfSerialization(elementType, serializationType), i);

            return true;
        }

        private static bool TryGetValueOfCollection(this Type serializationMemberType, object serializationMemberValue, out IList serializationCollection)
        {
            serializationCollection = null;

            Type elementType;
            if (serializationMemberType.IsArray || !typeof(IList).IsAssignableFrom(serializationMemberType) || !(elementType = serializationMemberType.GenericTypeArguments[0]).IsSerialization(out SerializationAttribute serializationAttr)) return false;

            var collection = (IList) serializationMemberValue;
            var serializationType = serializationAttr.SerializedType ?? elementType;

            serializationCollection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(serializationType));

            for (int i = 0; i < collection.Count; i++)
                serializationCollection.Add(collection[i].GetValueOfSerialization(elementType, serializationType));

            return true;
        }





        private static void SetValueOfSerialization(this object serializedObject, object serializationObject)
        {
            if (serializedObject == null || !serializedObject.GetType().TryGetMembersInfo(MemberTypes.Field | MemberTypes.Property, out MemberInfo[] members)) return;
            var serializedMembers = members.Where(member => member.IsSerialization<SerializedMemberAttribute>(out _)).ToDictionary(member => member, member => { member.IsSerialization(out SerializedMemberAttribute attr); return attr; });

            foreach (var memberInfo in serializedMembers)
                memberInfo.Key.SetFieldValue(memberInfo.Value.SerializedMemberName, serializedObject, serializationObject);
        }

        private static void SetFieldValue(this MemberInfo serializedMember, string serializationMemberName, object serializedObject, object serializationObject)
        {
            serializationMemberName = string.IsNullOrEmpty(serializationMemberName) ? serializedMember.Name : serializationMemberName;
            if (!serializationObject.GetType().TryGetMemberInfo(serializationMemberName, out MemberInfo serializationMember)) return;

            var serializationMemberType = serializationMember is FieldInfo info1 ? info1.FieldType : ((PropertyInfo)serializationMember).PropertyType;

            var serializedMemberValue = serializedMember.GetValue(serializedObject);

            if (serializedMemberValue != null)
            {
                if (serializationMemberType.TrySetValueOfDictionary(serializedMemberValue, out IDictionary dictionaryValue)) serializedMemberValue = dictionaryValue;
                else if (serializationMemberType.TrySetValueOfArray(serializedMemberValue, out Array arrayValue)) serializedMemberValue = arrayValue;
                else if (serializationMemberType.TrySetValueOfCollection(serializedMemberValue, out IList listValue)) serializedMemberValue = listValue;
                else if (serializationMemberType.IsSerialization<SerializationAttribute>(out _))
                {
                    serializedMemberValue.SetValueOfSerialization(serializationMember.GetValue(serializationObject));
                    return;
                }
            }

            serializationMember.SetValue(serializationObject, serializedMemberValue);
        }

        private static bool TrySetValueOfDictionary(this Type memberType, object memberValue, out IDictionary outDictionary)
        {
            outDictionary = null;
            if (!typeof(IDictionary).IsAssignableFrom(memberType)) return false;



            return true;
        }

        private static bool TrySetValueOfArray(this Type memberType, object memberValue, out Array outArray)
        {
            outArray = null;
            Type elementType;
            if (!memberType.IsArray || !(elementType = memberType.GetElementType()).IsSerialization<SerializationAttribute>(out _)) return false;

            var arrayValue = memberValue as Array;
            outArray = Array.CreateInstance(elementType, arrayValue.Length);

            for (int i = 0; i < arrayValue.Length; i++)
            {
                var serializationValue = Activator.CreateInstance(elementType, true);
                if (serializationValue == null) serializationValue = Activator.CreateInstance(elementType, false);
                arrayValue.GetValue(i).SetValueOfSerialization(serializationValue);
                outArray.SetValue(serializationValue, i);
            }

            return true;
        }

        private static bool TrySetValueOfCollection(this Type memberType, object memberValue, out IList outList)
        {
            outList = null;
            Type elementType;
            if (memberValue.GetType().IsArray || !(memberValue is IList list) || !(elementType = memberType.GenericTypeArguments[0]).IsSerialization<SerializationAttribute>(out _)) return false;

            outList = Activator.CreateInstance(memberType) as IList;

            for (int i = 0; i < list.Count; i++)
            {
                var serializationValue = Activator.CreateInstance(elementType, true);
                if (serializationValue == null) serializationValue = Activator.CreateInstance(elementType, false);
                list[i].SetValueOfSerialization(serializationValue);
                outList.Add(serializationValue);
            }

            return true;
        }

        private static void SetValue(this MemberInfo memberInfo, object typeObject, object value)
        {
            if (memberInfo is FieldInfo serializationField)
                serializationField.SetValue(typeObject, value);

            else if (memberInfo is PropertyInfo serializationProperty)
                serializationProperty.SetValue(typeObject, value);
        }

        private static object GetValue(this MemberInfo memberInfo, object typeObject)
        {
            if (memberInfo is FieldInfo serializationField)
                return serializationField.GetValue(typeObject);

            else if (memberInfo is PropertyInfo serializationProperty)
                return serializationProperty.GetValue(typeObject);

            else return null;
        }

        private static bool TryGetMemberInfo(this Type type, string name, out MemberInfo memberInfo, BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            memberInfo = null;

            while (type != null && type != typeof(object))
            {
                if (type.GetMember(name, bf) is MemberInfo[] membersInfo && membersInfo.Length > 0)
                {
                    memberInfo = membersInfo[0];
                    break;
                }

                type = type.BaseType;
            }

            return memberInfo != null;
        }

        private static bool TryGetMembersInfo<T>(this Type type, MemberTypes memberTypes, out T[] outMembersInfo, BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) where T : MemberInfo
        {
            List<Type> parents = new List<Type>();
            do {
                parents.Add(type);
                type = type.BaseType;
            } while (type != null && type != typeof(object));
            parents.Reverse();

            var outElementType = typeof(T);
            outMembersInfo = (T[])Array.CreateInstance(outElementType, 0);

            foreach (var parent in parents)
                foreach (var member in parent.GetMembers(bf))
                    if (!Array.Exists(outMembersInfo, m => m.Name == member.Name) && (memberTypes & member.MemberType) == member.MemberType)
                    {
                        Array.Resize(ref outMembersInfo, outMembersInfo.Length + 1);
                        outMembersInfo[outMembersInfo.Length - 1] = (T)member;
                    }

            return outMembersInfo.Length > 0;
        }

        private static bool IsSerialization<T>(this MemberInfo memberInfo, out T serialization) where T : Attribute
        {
            serialization = null;
            if (memberInfo == null || !memberInfo.IsDefined(typeof(T))) return false;

            serialization = memberInfo.GetCustomAttribute<T>();
            return true;
        }
    }
}
