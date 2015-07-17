using System.Globalization;
using Autofac;
using Autofac.Integration.Mvc;
using XsdTransformer.Core;

namespace XsdTransformer.Web.Site
{
    public class Bootstrapper
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.Register(c => new Core.XsdTransformer(c.Resolve<IXsdLineBuidler>()))
                .AsImplementedInterfaces()
                .InstancePerHttpRequest();
            builder.Register(c => new CsvXsdLineBuilder("  ", CultureInfo.CurrentCulture.TextInfo.ListSeparator))
                .AsImplementedInterfaces()
                .InstancePerHttpRequest();
            builder.Register(c => new XsdLoader())
                .AsImplementedInterfaces()
                .InstancePerHttpRequest();
        }
    }
}