using System;

namespace MLExample
{
    using MLExampleML.Model;
    using MLExampleML.Model.DataModels;

    class Program
    {
        static void Main()
        {
            var predictionEngineFactory = new PredictionEngineFactory();
            var predictionEngine = predictionEngineFactory.Create();
            while (true)
            {
                Console.Write("Text? ");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;
                var input = new ModelInput
                {
                    SentimentText = line
                };

                var prediction = predictionEngine.Predict(input);
                Console.WriteLine($"{(prediction.Prediction ? string.Empty : "not ")}toxic {prediction.Score}");
            }
        }
    }
}
