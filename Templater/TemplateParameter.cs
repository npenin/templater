namespace Templater
{
    public class TemplateParameter
    {
        public TemplateParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }

        public virtual bool IsFunction => false;
        public virtual bool IsScript => false;
    }

    public class ScriptTemplateParameter : TemplateParameter
    {
        public ScriptTemplateParameter(string name, object value)
            : base(name, value)
        {
        }

        public override bool IsScript => true;
    }

    public class ScriptToFunctionTemplateParameter : ScriptTemplateParameter
    {
        public ScriptToFunctionTemplateParameter(string name, object value)
            : base(name, value)
        {
        }

        public override bool IsFunction => true;
    }



    public class FunctionTemplateParameter<T> : TemplateParameter
    {
        public FunctionTemplateParameter(string name, T value) :
            base(name, value)
        {

        }

        public override bool IsFunction => true;
    }
}
