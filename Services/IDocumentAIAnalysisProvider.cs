using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public interface IDocumentAIAnalysisProvider
{
    DocumentAIAnalysisResult Analyze(ProjectDocument document);
}
