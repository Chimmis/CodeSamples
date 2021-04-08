using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace InputValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new ServiceCollection();

            // Two implementations of Ball validator are registered
            container.AddTransient<IValidator<Ball>, BallSizeValidator>()
                .AddTransient<IValidator<Ball>, BallPriceValidator>()
                .AddSingleton<RunnerService>()
                .AddSingleton<BallCreationService>()
                .AddSingleton<ValidationService<Ball>>();

            var runner = container.BuildServiceProvider().GetRequiredService<RunnerService>();
            runner.Run();
        }
    }

    public class RunnerService
    {
        private readonly BallCreationService ballCreationService;
        private readonly ValidationService<Ball> ballValidationService;

        public RunnerService(BallCreationService ballCreationService, ValidationService<Ball> ballValidationService)
        {
            this.ballCreationService = ballCreationService;
            this.ballValidationService = ballValidationService;
        }

        public void Run()
        {
            var balls = ballCreationService.CreateBalls();

            // Validate the balls with our created validators
            var validationOutputs = balls.SelectMany(ball => ballValidationService.Validate(ball));

            if (validationOutputs.Any(x => !x.Success))
            {
                foreach (var validationOutput in validationOutputs.Where(x => !x.Success))
                {
                    Console.WriteLine(string.Join(Environment.NewLine, validationOutput.Errors.Select(x => x.Message)));
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("All balls are valid!");
            }

            Console.Read();
        }
    }

    public class BallCreationService
    {
        public List<Ball> CreateBalls()
        {
            return new List<Ball>
            {
                new Ball(9, -10, "Free ball"),
                new Ball(1, 1001, "Plain wrong ball"),
                new Ball(5, 50, "Legit ball"),
                new Ball(3, 50000, "Pricey ball"),
            };
        }
    }

    public class ValidationService<T> where T: class
    {
        private readonly IEnumerable<IValidator<T>> validators;

        public ValidationService(IEnumerable<IValidator<T>> validators)
        {
            this.validators = validators;
        }

        public List<ValidationOutput> Validate(T input)
        {
            return validators.Select(validator => validator.Validate(input)).ToList();
        }
    }

    public class Ball
    {
        public string Name { get; set; }
        public int Size { get; set; }

        public int Price { get; set; }

        public Ball(int size, int price, string name)
        {
            this.Size = size;
            this.Price = price;
            this.Name = name;
        }
    }

    public class BallSizeValidator : IValidator<Ball>
    {
        public ValidationOutput Validate(Ball input)
        {
            var errors = new List<ValidationError>();
            if (input.Size < 2)
            {
                errors.Add(ValidationError.Create($"{input.Name} : Ball is too small, it needs to be larger than 2."));
            }

            if (input.Size > 10)
            {
                errors.Add(ValidationError.Create($"{input.Name} : Ball is too large, it needs to be smaller than 10."));
            }

            return ValidationOutput.Create(errors);
        }
    }

    public class BallPriceValidator : IValidator<Ball>
    {
        public ValidationOutput Validate(Ball input)
        {
            var errors = new List<ValidationError>();
            if (input.Price <= 0)
            {
                errors.Add(ValidationError.Create($"{input.Name} : Ball price needs to be larger than 0, we'll go bankrupt if we give shit away for free."));
            }

            if (input.Price > 1000)
            {
                errors.Add(ValidationError.Create($"{input.Name} : No ball is that expensive ({input.Price}), it needs to be less than 1000."));
            }

            return ValidationOutput.Create(errors);
        }
    }


    public interface IValidator<T> 
        where T: class
    {
        ValidationOutput Validate(T input);
    }

    public class ValidationOutput
    {
        public List<ValidationError> Errors { get; }

        public bool Success => Errors.Count == 0;

        private ValidationOutput(List<ValidationError> errors)
        {
            this.Errors = errors;
        }

        public static ValidationOutput Create (List<ValidationError> errors)
        {
            return new ValidationOutput(errors);
        }
    }

    public class ValidationError
    {
        // This structure ensures, that once the message is created - it can not be changed
        public string Message { get; }

        private ValidationError(string message)
        {
            this.Message = message;
        }

        public static ValidationError Create(string message)
        {
            return new ValidationError(message);
        }
    }
}
