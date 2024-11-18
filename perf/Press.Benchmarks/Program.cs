using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = DefaultConfig.Instance
    .WithOption(ConfigOptions.DisableOptimizationsValidator, true); // IronPdf :/

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
