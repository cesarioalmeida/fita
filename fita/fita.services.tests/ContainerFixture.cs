using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DryIoc;
using DryIoc.MefAttributedModel;
using twentySix.Framework.Core.Helpers;

namespace fita.services.tests;

public class ContainerFixture
{
    public ContainerFixture()
    {
        ApplicationHelper.SetApplicationDetails("twentySix", "fita.tests");
        ApplicationHelper.StartUp(new Container().WithMef().WithMefAttributedModel());

        Container.RegisterExports(GetAssemblies());
    }

    public static IContainer Container => ApplicationHelper.Container;

    private static IEnumerable<Assembly> GetAssemblies()
        => from frameworkAssembly in new[]
            {
                "twentySix.Framework.Core.dll",
                "twentySix.Framework.Theme.dll",
                "fita.data.dll",
                "fita.services.dll"
            }
            select ApplicationHelper.GetFullPath(frameworkAssembly)
            into fullPath
            where File.Exists(fullPath)
            select Assembly.LoadFrom(fullPath);
}