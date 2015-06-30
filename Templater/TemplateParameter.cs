using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

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

        public virtual bool IsFunction { get { return false; } }
        public virtual bool IsScript { get { return false; } }
    }

    public class ScriptTemplateParameter : TemplateParameter
    {
        public ScriptTemplateParameter(string name, object value)
            : base(name, value)
        {
        }

        public override bool IsScript { get { return true; } }
    }

    public class ScriptToFunctionTemplateParameter : ScriptTemplateParameter
    {
        public ScriptToFunctionTemplateParameter(string name, object value)
            : base(name, value)
        {
        }

        public override bool IsFunction { get { return true; } }
    }



    public class FunctionTemplateParameter<T> : TemplateParameter
    {
        public FunctionTemplateParameter(string name, T value) :
            base(name, value)
        {

        }

        public override bool IsFunction { get { return true; } }
    }
}
