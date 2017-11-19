namespace Reusable.Console
{
    public class PlayingWithEnvDTE
    {
        
    }
    
     //class foo
    //{
    //    private void Start(Type interfaceToFind, Predicate<string> propertySelection)
    //    {
    //        //_interfaceToFind = interfaceToFind.Name;
    //        //_propertySelection = propertySelection;

    //        //IServiceProvider hostServiceProvider = (IServiceProvider)Host;
    //        EnvDTE.DTE dte = (EnvDTE.DTE)hostServiceProvider.GetService(typeof(EnvDTE.DTE));

    //        EnvDTE.ProjectItem containingProjectItem = dte.Solution.FindProjectItem("app.config");
    //        Project project = containingProjectItem.ContainingProject;

    //        foreach (ProjectItem pi in project.ProjectItems)
    //        {
    //            ProcessProjectItem(pi);
    //        }
    //    }

    //    private void ProcessProjectItem(ProjectItem pi)
    //    {
    //        FileCodeModel fcm = pi.FileCodeModel;

    //        if (fcm != null)
    //        {
    //            foreach (CodeElement ce in fcm.CodeElements)
    //            {
    //                CrawlElement(ce);
    //            }
    //        }

    //        if (pi.ProjectItems != null)
    //        {
    //            foreach (ProjectItem prji in pi.ProjectItems)
    //            {
    //                ProcessProjectItem(prji);
    //            }
    //        }
    //    }

    //    private void CrawlElement(CodeElement codeElement)
    //    {
    //        switch (codeElement.Kind)
    //        {

    //            case vsCMElement.vsCMElementNamespace:
    //            {
    //                ProcessNamespace(codeElement);
    //                break;
    //            }
    //            case vsCMElement.vsCMElementClass:
    //            {
    //                ProcessClass(codeElement);
    //                break;
    //            }
    //            default:
    //            {
    //                return;
    //            }
    //        }
    //    }

    //    private void ProcessNamespace(CodeElement codeElement)
    //    {
    //        foreach (CodeElement m in ((CodeNamespace)codeElement).Members)
    //        {
    //            CrawlElement(m);
    //        }
    //    }

    //    private void ProcessClass(CodeElement codeElement)
    //    {
    //        CodeClass2 codeClass = (CodeClass2)codeElement;

    //        if (codeClass.ClassKind != vsCMClassKind.vsCMClassKindPartialClass) { return; }

    //        foreach (var i in codeClass.ImplementedInterfaces)
    //        {
    //            if (((CodeElement)i).Name.Equals(_interfaceToFind))
    //            {
    //                GenerateCode(codeClass);
    //            }
    //        }
    //    }

    //    private void GenerateCode(CodeClass classObj)
    //    {
    //        WriteClassStart(classObj.Name, GetAccessString(classObj), classObj.Namespace.Name);

    //        GenerateProperties(classObj);

    //        WriteClassEnd(classObj.Name, GetAccessString(classObj), classObj.Namespace.Name);
    //    }

    //    private void GenerateProperties(CodeClass classObj)
    //    {
    //        foreach (CodeElement childElement in classObj.Children)
    //        {
    //            if (childElement.Kind != vsCMElement.vsCMElementVariable) { continue; }

    //            CodeVariable childVariable = childElement as CodeVariable;

    //            if (!_propertySelection(childVariable.Name)) { continue; }

    //            WriteProperty(childVariable.Name, childVariable.Type.CodeType.FullName);
    //        }
    //    }

    //    private string GetAccessString(CodeClass classObj)
    //    {
    //        string access = string.Empty;
    //        switch (classObj.Access)
    //        {
    //            case vsCMAccess.vsCMAccessPublic: { access = "public"; break; }
    //            case vsCMAccess.vsCMAccessPrivate: { access = "private"; break; }
    //            case vsCMAccess.vsCMAccessDefault: { access = "public"; break; }
    //            case vsCMAccess.vsCMAccessProtected: { access = "protected"; break; }
    //            case vsCMAccess.vsCMAccessAssemblyOrFamily: { access = "internal"; break; }
    //            default: { access = "internal"; break; }
    //        }

    //        return access;
    //    }
    //}
}