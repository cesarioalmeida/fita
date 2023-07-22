using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DryIoc;
using DryIoc.MefAttributedModel;
using Moq;
using twentySix.Framework.Core.Helpers;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.tests;

public class ContainerFixture
{
    public ContainerFixture()
    {
        ApplicationHelper.SetApplicationDetails("twentySix", "fita.tests");

        var container = new Container().WithMef().WithMefAttributedModel();
        container.RegisterExports(GetAssemblies());
        EnrichContainer(container);

        ApplicationHelper.StartUp(container);
        
        container.InjectPropertiesAndFields(this);
    }

    private static void EnrichContainer(IRegistrator container)
    {
        // replace the logging service with mocked version
        container.Unregister<ILoggingService>();
        container.Register<ILoggingService>(Reuse.Singleton, Made.Of(() => Mock.Of<ILoggingService>()));
    }

    public static IContainer Container => ApplicationHelper.Container;

    private static IEnumerable<Assembly> GetAssemblies()
        => from frameworkAssembly in new[]
            {
                "twentySix.Framework.Core.dll",
                "fita.data.dll",
                "fita.services.dll"
            }
            select ApplicationHelper.GetFullPath(frameworkAssembly)
            into fullPath
            where File.Exists(fullPath)
            select Assembly.LoadFrom(fullPath);
}