using System.Globalization;
using Autofac;
using XsdTransformer.Core;

namespace XsdTransformer.Console
{
    public class Bootstrapper
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => new Core.XsdTransformer(c.Resolve<IXsdLineBuidler>()))
                .AsImplementedInterfaces();
            builder.Register(c => new CsvXsdLineBuilder("  ", CultureInfo.CurrentCulture.TextInfo.ListSeparator))
                .AsImplementedInterfaces();
            builder.Register(c => new XsdLoader())
                .AsImplementedInterfaces();

            return builder.Build();
        }
    }
}