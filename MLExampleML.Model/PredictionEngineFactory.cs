using Microsoft.ML;
using MLExampleML.Model.DataModels;

namespace MLExampleML.Model
{
    public class PredictionEngineFactory
    {
        public PredictionEngine<ModelInput, ModelOutput> Create()
        {
            var assembly = typeof(PredictionEngineFactory).Assembly;
            var mlContext = new MLContext();
            using (var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.MLModel.zip"))
            {
                var model = mlContext.Model.Load(stream, out _);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
                return predictionEngine;
            }
        }
    }
}
