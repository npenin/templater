using Jurassic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Templater
{
    /// <summary>
    /// Mail service exposing static methods to send mails
    /// </summary>
    public class TemplateJurassicService : TemplateService<ScriptEngine>
    {
        public TemplateJurassicService()
            : base(TemplateMode.Standard)
        {

        }

        public TemplateJurassicService(string startExpressionToken, string startStatementToken, string endToken)
            : base(startExpressionToken, startStatementToken, endToken)
        {
        }

        public TemplateJurassicService(TemplateMode mode)
            : base(mode)
        {
        }


        public override ScriptEngine CreateEngine(StringBuilder output)
        {
            ScriptEngine engine = new ScriptEngine();
            engine.EnableExposedClrTypes = true;

            engine.SetGlobalFunction("write", new Action<object>(s =>
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

        public override void SetParameters(ScriptEngine engine, IEnumerable<TemplateParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (TemplateParameter parameter in parameters)
                {
                    if (parameter.IsFunction && parameter.Value != null)
                    {
                        if (parameter.IsScript)
                            engine.SetGlobalValue(parameter.Name, engine.Function.Construct(parameter.Value.ToString()));
                        else
                            engine.SetGlobalFunction(parameter.Name, (Delegate)parameter.Value);
                    }
                    else if (parameter.IsScript && parameter.Value != null)
                    {
                        parameter.Value = GenerateScriptTemplate(parameter.Value.ToString());
                        engine.SetGlobalValue(parameter.Name, parameter);
                    }
                    else
                    {
                        engine.SetGlobalValue(parameter.Name, parameter.Value);
                    }
                }
            }
        }

        protected override void Execute(ScriptEngine engine, string script)
        {
            engine.Execute(script);
        }
    }
}
