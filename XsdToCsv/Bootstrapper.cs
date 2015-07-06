using System.Globalization;
using Autofac;

namespace XsdToCsv
{
    public class Bootstrapper
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => new XsdTransformer(c.Resolve<IXsdObjectFinder>(), c.Resolve<IXsdLineBuidler>()))
                .AsImplementedInterfaces();
            builder.Register(c => new CsvXsdLineBuilder("  ", CultureInfo.CurrentCulture.TextInfo.ListSeparator))
                .AsImplementedInterfaces();
            builder.Register(c => new XsdObjectFinder())
                .AsImplementedInterfaces();
            builder.Register(c => new XsdLoader())
                .AsImplementedInterfaces();

            return builder.Build();
        }
    }
}