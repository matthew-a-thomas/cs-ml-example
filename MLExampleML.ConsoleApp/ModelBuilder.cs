//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using MLExampleML.Model.DataModels;
using Microsoft.ML.Trainers;

namespace MLExampleML.ConsoleApp
{
    using System.Diagnostics.CodeAnalysis;

    public static class ModelBuilder
    {
        private static string _trainDataFilepath = @"E:\Google One\Dropbox\Personal\Software\Programming\Projects\MLExample\MLExample\wikipedia-detox-250-line-data.tsv";
        private static string _modelFilepath = @"../../../../MLExampleML.Model/MLModel.zip";

        // Create MLContext to be shared across the model creation workflow objects
        // Set a random seed for repeatable/deterministic results across multiple trainings.
        private static readonly MLContext MlContext = new MLContext(seed: 1);

        public static void CreateModel()
        {
            // Load Data
            var trainingDataView = MlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: _trainDataFilepath,
                                            hasHeader: true,
                                            separatorChar: '\t',
                                            allowQuoting: true);

            // Build training pipeline
            var trainingPipeline = BuildTrainingPipeline(MlContext);

            // Evaluate quality of Model
            Evaluate(MlContext, trainingDataView, trainingPipeline);

            // Train Model
            var mlModel = TrainModel(MlContext, trainingDataView, trainingPipeline);

            // Save model
            SaveModel(MlContext, mlModel, _modelFilepath, trainingDataView.Schema);
        }

        public static IEstimator<ITransformer> BuildTrainingPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("SentimentText_tf", "SentimentText")
                                      .Append(mlContext.Transforms.CopyColumns("Features", "SentimentText_tf"))
                                      .Append(mlContext.Transforms.NormalizeMinMax("Features", "Features"))
                                      .AppendCacheCheckpoint(mlContext);

            // Set the training algorithm
            var trainer = mlContext.BinaryClassification.Trainers.SgdCalibrated(new SgdCalibratedTrainer.Options() { L2Regularization = 5E-06f, ConvergenceTolerance = 1E-05f, NumberOfIterations = 10, Shuffle = true, LabelColumnName = "Sentiment", FeatureColumnName = "Features" });
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            return trainingPipeline;
        }

        public static ITransformer TrainModel(MLContext mlContext, IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
        {
            Console.WriteLine("=============== Training  model ===============");

            var model = trainingPipeline.Fit(trainingDataView);

            Console.WriteLine("=============== End of training process ===============");
            return model;
        }

        private static void Evaluate(MLContext mlContext, IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
        {
            // Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            var crossValidationResults = mlContext.BinaryClassification.CrossValidateNonCalibrated(trainingDataView, trainingPipeline, numberOfFolds: 5, labelColumnName: "Sentiment");
            PrintBinaryClassificationFoldsAverageMetrics(crossValidationResults);
        }
        private static void SaveModel(MLContext mlContext, ITransformer mlModel, string modelRelativePath, DataViewSchema modelInputSchema)
        {
            // Save/persist the trained model to a .ZIP file
            Console.WriteLine($"=============== Saving the model  ===============");
            mlContext.Model.Save(mlModel, modelInputSchema, GetAbsolutePath(modelRelativePath));
            Console.WriteLine("The model is saved to {0}", GetAbsolutePath(modelRelativePath));
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = dataRoot.Directory?.FullName;

            var fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        public static void PrintBinaryClassificationMetrics(BinaryClassificationMetrics metrics)
        {
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*       Metrics for binary classification model      ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"*       Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"*       Auc:      {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"************************************************************");
        }


        public static void PrintBinaryClassificationFoldsAverageMetrics(IEnumerable<TrainCatalogBase.CrossValidationResult<BinaryClassificationMetrics>> crossValResults)
        {
            var metricsInMultipleFolds = crossValResults.Select(r => r.Metrics);

            var accuracyValues = metricsInMultipleFolds.Select(m => m.Accuracy).ToList();
            var accuracyAverage = accuracyValues.Average();
            var accuraciesStdDeviation = CalculateStandardDeviation(accuracyValues);
            var accuraciesConfidenceInterval95 = CalculateConfidenceInterval95(accuracyValues);


            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*       Metrics for Binary Classification model      ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       Average Accuracy:    {accuracyAverage:0.###}  - Standard deviation: ({accuraciesStdDeviation:#.###})  - Confidence Interval 95%: ({accuraciesConfidenceInterval95:#.###})");
            Console.WriteLine($"*************************************************************************************************************");
        }

        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global")]
        public static double CalculateStandardDeviation(IReadOnlyCollection<double> values)
        {
            var average = values.Average();
            var sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
            var standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / (values.Count() - 1));
            return standardDeviation;
        }

        public static double CalculateConfidenceInterval95(IReadOnlyCollection<double> values)
        {
            var confidenceInterval95 = 1.96 * CalculateStandardDeviation(values) / Math.Sqrt((values.Count() - 1));
            return confidenceInterval95;
        }
    }
}
