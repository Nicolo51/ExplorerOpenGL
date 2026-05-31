using System;

namespace ExplorerOpenGL.Model
{
    public class Constant
    {
        public Type Type { get; set; }
        public object Value { get; set; }
        public string Description { get; set; }
        public ConstantType ConstantType { get; set; }


        public Constant(Type type, object value, string description, ConstantType valueType = ConstantType.Option)
        {
            Type = type;
            Value = value;
            Description = description;
            ConstantType = valueType; 
        }

        public T GetValue<T>()
        {
            return (T)Value; 
        }

        public void Set(object value)
        {
            Value = Convert.ChangeType(value, Type);
        }

    }

    public enum ConstantType
    {
        Option, 
        AutoComp, 
        Both
    }
}
