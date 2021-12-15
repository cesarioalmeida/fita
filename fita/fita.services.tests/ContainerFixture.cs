using System;
using System.Reflection;
using LightInject;
using Moq;
using twentySix.Framework.Core.Helpers;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.tests;

public class ContainerFixture : IDisposable
{
    public ContainerFixture()
    {
        ApplicationHelper.SetApplicationDetails("twentySix", "fita.tests");

        var container = CreateContainer();
        Configure(container);

        container.RegisterAssembly("twentySix.Framework.*.dll");
        container.RegisterAssembly("fita.common.dll");
        container.RegisterAssembly("fita.data.dll");
        container.RegisterAssembly("fita.services.dll");

        // mocked services
        container.Register(_ => new Mock<ILoggingService>().Object);

        ServiceFactory = container.BeginScope();
        InjectPrivateFields();
    }

    private void InjectPrivateFields()
    {
        var privateInstanceFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var privateInstanceField in privateInstanceFields)
        {
            privateInstanceField.SetValue(this, GetInstance(privateInstanceField));
        }
    }

    internal Scope ServiceFactory { get; }

    public void Dispose() => ServiceFactory.Dispose();

    public TService GetInstance<TService>(string name = "") 
        => ServiceFactory.GetInstance<TService>(name);

    private object GetInstance(FieldInfo field)
        => ServiceFactory.TryGetInstance(field.FieldType) ?? ServiceFactory.GetInstance(field.FieldType, field.Name);

    internal IServiceContainer CreateContainer() => new ServiceContainer();

    internal void Configure(IServiceRegistry serviceRegistry)
    {
    }
}