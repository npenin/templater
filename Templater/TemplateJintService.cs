using Acornima.Ast;
using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Text;

namespace Templater
{
    /// <summary>
    /// Mail service exposing static methods to send mails
    /// </summary>
    public class TemplateJintService : TemplateService<Engine>
    {
        public TemplateJintService()
            : base(TemplateMode.Standard)
        {

        }

        public TemplateJintService(string startExpressionToken, string startStatementToken, string endToken)
            : base(startExpressionToken, startStatementToken, endToken)
        {
        }

        public TemplateJintService(TemplateMode mode)
            : base(mode)
        {
        }

        public Engine CreateEngine(StringBuilder output, Action<Options> options)
        {
            Engine engine = new Engine(options);

            engine.SetValue("write", new Action<object>(s =>
            {
                if (s is TemplateParameter)
                {
                    engine.Execute((string)((TemplateParameter)s).Value);
                }
                else
                {
                    output.Append(s);
                }
            }));

            RaisePrepareEngine(engine);

            return engine;
        }

        public override Engine CreateEngine(StringBuilder output)
        {
            return CreateEngine(output, (options) => { });
        }

        public override void SetParameters(Engine engine, IEnumerable<TemplateParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (TemplateParameter parameter in parameters)
                {
                    if (parameter.IsFunction && parameter.Value != null)
                    {
                        if (parameter.IsScript)
                        {
                            var script = Engine.PrepareScript(parameter.Value.ToString(), "templateParam." + parameter.Name);
                            // Updated for Jint 4.4.2 - Esprima is no longer used
                            engine.SetValue(parameter.Name, (Action<JsValue>)((templater) => engine.Evaluate(script)));
                        }
                        else
                        {
                            engine.SetValue(parameter.Name, (Delegate)parameter.Value);
                        }
                    }
                    else if (parameter.IsScript && parameter.Value != null)
                    {
                        parameter.Value = GenerateScriptTemplate(parameter.Value.ToString());
                        engine.SetValue(parameter.Name, parameter);
                    }
                    else
                    {
                        engine.SetValue(parameter.Name, parameter.Value);
                    }
                }
            }
        }

        protected override void Execute(Engine engine, string script)
        {
            engine.Execute(script);
        }
    }
}
