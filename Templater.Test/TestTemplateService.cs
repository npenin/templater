using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;

namespace Templater.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestTemplateService
    {
        public TestTemplateService()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestBasicUsage()
        {
            Assert.AreEqual("pwet", new TemplateService().Process("<%= pwic %>", new TemplateParameter("pwic", "pwet")));
            Assert.AreEqual("pwet", new TemplateService(TemplateMode.HtmlEscaped).Process("&lt;%= pwic %&gt;", new TemplateParameter("pwic", "pwet")));
        }

        [TestMethod]
        public void TestScriptTransformation()
        {
            TemplateService templateService = new TemplateService(TemplateMode.HtmlEscaped);
            templateService.TransformFoundScript += new Func<string, string>(templateService_TransformFoundScript);
            Assert.AreEqual("pwet", templateService.Process("&lt;% if(1&lt;2) write(&quot;pwet&quot;); %&gt;"));
            //Assert.AreEqual("pwet", templateService.Process("&lt;% if(1&lt;2) %&lt; &lt;%= 'pwet' %&gt;"));
            templateService = new TemplateService(TemplateMode.Standard);
            templateService.TransformFoundScript += new Func<string, string>(templateService_TransformFoundScript);
            Assert.AreEqual("pwet", templateService.Process("<% if(2>1) { %><%= 'pwet' %><% } %><% else if(2>1) { %><%= 'pwic' %><% } %>"));
            Assert.AreEqual("pwic", templateService.Process("<% if(1>2) { %><%= 'pwet' %><% } %><% else if(2>1) { %><%= 'pwic' %><% } %>"));
        }

        [TestMethod]
        public void TestEnginePreparation()
        {
            TemplateService templateService = new TemplateService();
            templateService.PrepareEngine += new System.Action<Jint.Engine>(templateService_PrepareEngine);
            Assert.AreEqual("pwet", templateService.Process("<% write('pwet'); %>"));
            templateService = new TemplateService(TemplateMode.HtmlEscaped);
            templateService.PrepareEngine += new System.Action<Jint.Engine>(templateService_PrepareEngine);
            Assert.AreEqual("pwet", templateService.Process("&lt;% write2('pwet'); %&gt;"));
        }



        void templateService_PrepareEngine(Jint.Engine engine)
        {
            engine.SetValue("write2", new Action<string>(s => engine.GetValue("write").As<Jint.Native.Function.FunctionInstance>().Call(engine.Global, new[] { new Jint.Native.JsValue(s) })));
        }

        string templateService_TransformFoundScript(string arg)
        {
            return HttpUtility.HtmlDecode(arg);
        }
    }
}
