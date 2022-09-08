using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Resource
{
    [Serialization(typeof(MyViewResource))]
    class ViewResource
    {
        [SerializeField] private int m_Id = 0;
        [SerializeField] private string m_Text = "";
        [SerializeField] private Color m_Color = Color.white;
        
        private Color mTextColor = Color.white;

        public int Id { get => m_Id; set => m_Id = value; }
        public string Text { get => m_Text; set => m_Text = value; }
        public Color Color { get => m_Color; 
            set
            {
                m_Color = value; 
                mTextColor = m_Color.DiscolorToHSV().z > .53f ? new Color(.2f, .2f, .2f, 1f) : new Color(.9f, .9f, .9f, 1f);
            }
        }
        public Color TextColor => mTextColor;

        public static implicit operator MyViewResource(ViewResource viewResource) =>
            new MyViewResource { id = viewResource.m_Id, text = viewResource.m_Text, color = viewResource.m_Color };

        public MyColor SerializationColor { get => (MyColor)Color; set => Color = (Color)value; }

        private ViewResource() { }

        public ViewResource(int id) => Set(id, Text, Color);

        public ViewResource(int id, Color color) => Set(id, color);

        public ViewResource(int id, string text, Color color) => Set(id, text, color);

        public ViewResource(ViewResource resources) => Set(resources.Id, resources.Text, resources.m_Color);

        public ViewResource Set(int id, Color color) => Set(id, Text, color);

        public ViewResource Set(int id, string text) => Set(id, text, Color);

        public ViewResource Set(int id, string text,  Color color)
        {
            Id = id;
            Text = text;
            Color = color;

            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ViewResource resource)) return false;
            else if (resource == this) return true;

            return resource.m_Id == m_Id && resource.m_Text == m_Text && resource.m_Color == m_Color;
        }

        public static List<ViewResource> GenerateResources(int count)
        {
            var resources = new List<ViewResource>();

            for (var i = 0; i < count; i++)
            {
            returnRandomColor:
                var color = UnityEngine.Random.ColorHSV(0f, 1f, .25f, 1f, .25f, 1f);
                if (i > 0 && !resources.TrueForAll(res => res.Color.CheckTheColors(color)))
                    goto returnRandomColor;

                resources.Add(new ViewResource(i, color));
            }

            return resources;
        }

        public static List<ViewResource> CreateMultiple(List<ViewResource> directoryResources)
        {
            var resources = new List<ViewResource>();
            var count = (int)Mathf.Pow(directoryResources.Count, 2);

            for (var i = 0; i < count; i++)
                resources.Add(new ViewResource(directoryResources[i % directoryResources.Count])
                    .Set(i + 1, (i / directoryResources.Count + 1).ToString()));

            return resources;
        }
    }

    [Serializable] struct MyViewResource
    {
        [SerializedMember("Id")] public int id;
        [SerializedMember("Text")] public string text;
        [SerializedMember("SerializationColor")] public MyColor color;

        public static explicit operator ViewResource(MyViewResource myViewResource) =>
            new ViewResource(myViewResource.id, myViewResource.text, (Color) myViewResource.color);
    }
    [Serializable] internal struct MyColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static implicit operator MyColor(Color color) =>
            new MyColor { r = color.r, g = color.g, b = color.b, a = color.a };

        public static explicit operator Color(MyColor color) =>
            new Color { r = color.r, g = color.g, b = color.b, a = color.a };
    }
}
