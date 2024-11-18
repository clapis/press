using System.Text;
using BenchmarkDotNet.Attributes;

namespace Press.Benchmarks;

[DryJob]
[MemoryDiagnoser]
public class Extractors
{
    private const string FilePath = "./Assets/dou.pdf";
    
    [Benchmark]
    public string PdfPigLib()
    {
        var stream = new FileStream(FilePath, FileMode.Open);
        var doc = UglyToad.PdfPig.PdfDocument.Open(stream);
    
        var result = new StringBuilder();
    
        for (int i = 1; i <= doc.NumberOfPages; i++)
            result.Append(string.Join(" ", doc.GetPage(i).GetWords().Select(x => x.Text)));
        
        return result.ToString();
    }
    
    [Benchmark]
    public string DocnetLib()
    {
        var result = new StringBuilder();
        
        using var docReader = Docnet.Core.DocLib.Instance.GetDocReader(FilePath, new Docnet.Core.Models.PageDimensions());
        
        for (var i = 0; i < docReader.GetPageCount(); i++)
        {
            using (var pageReader = docReader.GetPageReader(i))
                result.Append(pageReader.GetText());
        }
        
        return result.ToString();
    }
    
    [Benchmark]
    public string iTextLib()
    {
        var result = new StringBuilder();
    
        using var reader = new iText.Kernel.Pdf.PdfReader(FilePath);
    
        using var doc = new iText.Kernel.Pdf.PdfDocument(reader);
    
        for (int i = 1; i < doc.GetNumberOfPages(); i++)
            result.Append(iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(doc.GetPage(i)));
        
        return result.ToString();
    }
    
    [Benchmark]
    public string AsposeLib()
    {
        using var doc = new Aspose.Pdf.Document(FilePath);
        
        var absorber = new Aspose.Pdf.Text.TextAbsorber();
    
        doc.Pages.Accept(absorber);
        
        return absorber.Text;
    }
    
    [Benchmark]
    public string SyncFusionLib()
    {
        using var stream = new FileStream(FilePath, FileMode.Open);
        using var doc = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(stream);
        
        var result = new StringBuilder();
    
        for (int i = 0; i < doc.Pages.Count; i++)
            result.Append(doc.Pages[i].ExtractText());
        
        return result.ToString();
    }
    
    [Benchmark]
    public string IronPdfLib()
    {
        IronPdf.License.LicenseKey = "IRONSUITE.GENIBO9656.CASHBN.COM.4838-F00A3FB832-GLLN2IPOKMQPZ2-2EMRRSMYREN5-CKAUMCP2UNTJ-C3YUXKMJCJA6-YLVYLEMTQ76Y-VTKZ7IWVSUGL-JTPA3D-TRTMVQXFDK2OEA-DEPLOYMENT.TRIAL-66HTGQ.TRIAL.EXPIRES.18.DEC.2024";
        
        using var doc = new IronPdf.PdfDocument(FilePath);
    
        return doc.ExtractAllText();
    }
    
    [Benchmark]
    public string DocoticLib()
    {
        BitMiracle.Docotic.LicenseManager.AddLicenseData("9S25N-ZKV8E-DJGLE-X3QQL-X1MCQ");
        
        using var doc = new BitMiracle.Docotic.Pdf.PdfDocument(FilePath);
    
        return doc.GetText();
    }
}