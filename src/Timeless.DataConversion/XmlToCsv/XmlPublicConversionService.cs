using System.Text;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlPublicConversionService
{
    private readonly XmlConversionLimits _limits;

    public XmlPublicConversionService(XmlConversionLimits limits)
    {
        _limits = limits ?? new XmlConversionLimits();
        _limits.RejectAmbiguousPlans = true;
    }

    public XmlConversionPreview CreatePreview(string xmlSourceFilePath)
    {
        return XmlToCsvConverter.CreateConversionPreview(xmlSourceFilePath, _limits);
    }

    public XmlInferredTablePlan ConfirmPlan(XmlConversionPreview preview, XmlConversionPlanConfirmation confirmation)
    {
        XmlInferredTablePlan plan = XmlToCsvConverter.ConfirmConversionPlan(preview, confirmation);
        XmlConversionValidationResult validationResult = XmlToCsvConverter.ValidateTablePlan(plan, _limits);

        if (!validationResult.IsValid)
        {
            throw new XmlConversionValidationException(validationResult);
        }

        return plan;
    }

    public void ExportConfirmedConversion(
        string xmlSourceFilePath,
        string destinationDirectory,
        Encoding encoding,
        XmlConversionPreview preview,
        XmlConversionPlanConfirmation confirmation)
    {
        XmlToCsvConverter.ExportConfirmedConversion(xmlSourceFilePath, destinationDirectory, encoding, preview, confirmation, _limits);
    }

    public void ExportConfirmedConversionToZip(
        string xmlSourceFilePath,
        string destinationZipPath,
        Encoding encoding,
        XmlConversionPreview preview,
        XmlConversionPlanConfirmation confirmation)
    {
        XmlToCsvConverter.ExportConfirmedConversionToZip(xmlSourceFilePath, destinationZipPath, encoding, preview, confirmation, _limits);
    }
}
