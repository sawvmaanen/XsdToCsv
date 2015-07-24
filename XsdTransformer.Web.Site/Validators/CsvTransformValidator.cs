using System.Linq;
using FluentValidation;
using XsdTransformer.Web.Site.Models;

namespace XsdTransformer.Web.Site.Validators
{
    public class CsvTransformValidator : AbstractValidator<CsvTransformModel>
    {
        public CsvTransformValidator()
        {
            RuleFor(m => m.Files)
                .NotEmpty();

            RuleFor(m => m.XmlFileName)
                .NotEmpty()
                .Must((m, f) => m.Files != null && m.Files.Any(x => x.FileName == f))
                .WithMessage("Not part of uploaded files.");

            RuleFor(m => m.XsdFileName)
                .NotEmpty()
                .Must((m, f) => m.Files != null && m.Files.Any(x => x.FileName == f))
                .WithMessage("Not part of uploaded files.");
        }
    }
}