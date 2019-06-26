//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using MLExampleML.Model.DataModels;


namespace MLExampleML.ConsoleApp
{
    class Program
    {
        //Machine Learning model to load and use for predictions
        private const string ModelFilepath = @"MLModel.zip";

        //Dataset to use for predictions
        private const string DataFilepath = @"E:\Google One\Dropbox\Personal\Software\Programming\Projects\MLExample\MLExample\wikipedia-detox-250-line-data.tsv";

        static void Main()
        {
            var mlContext = new MLContext();

            // Training code used by ML.NET CLI and AutoML to generate the model
            //ModelBuilder.CreateModel();

            var mlModel = mlContext.Model.Load(GetAbsolutePath(ModelFilepath), out _);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            // Create sample data to do a single prediction with it
            var sampleData = CreateSingleDataSample(mlContext, DataFilepath);

            // Try a single prediction
            var predictionResult = predEngine.Predict(sampleData);

            Console.WriteLine($"Single Prediction --> Actual value: {sampleData.Sentiment} | Predicted value: {predictionResult.Prediction}");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        // Method to load single row of data to try a single prediction
        // You can change this code and create your own sample data here (Hardcoded or from any source)
        private static ModelInput CreateSingleDataSample(MLContext mlContext, string dataFilePath)
        {
            // Read dataset to get a single row for trying a prediction
            var dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: dataFilePath,
                                            hasHeader: true,
                                            separatorChar: '\t',
                                            allowQuoting: true);

            // Here (ModelInput object) you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
            var sampleForPrediction = mlContext.Data.CreateEnumerable<ModelInput>(dataView, false)
                                                                        .First();
            return sampleForPrediction;
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = dataRoot.Directory?.FullName;

            var fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
