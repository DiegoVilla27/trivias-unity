using UnityEditor;
using UnityEngine;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Asigna campos privados [SerializeField] desde los constructores de edición, igual que arrastrar en el Inspector.
    /// </summary>
    public static class SerializedWiring
    {
        public static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            Find(so, target, field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Set(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            Find(so, target, field).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Set(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            Find(so, target, field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Set(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            Find(so, target, field).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Set(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            Find(so, target, field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var property = Find(so, target, field);
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty Find(SerializedObject so, Object target, string field)
        {
            var property = so.FindProperty(field);
            if (property == null)
                throw new System.ArgumentException($"El campo '{field}' no existe en {target.GetType().Name}.");
            return property;
        }
    }
}
