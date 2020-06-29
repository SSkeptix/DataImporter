using DataImporter.FileHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Services
{
    public interface IFileParser
    {
        FileExtension FileExtension { get; }

        Type DataType { get; }
    }

    public interface IFileParser<T> : IFileParser
        where T : class
    {
        Task<T[]> Parse(Stream stream);
    }

    public abstract class FileParser<TEntity> : IFileParser<TEntity>
        where TEntity : class
    {
        public Type DataType => typeof(TEntity);

        public abstract FileExtension FileExtension { get; }

        public abstract Task<TEntity[]> Parse(Stream stream);

        protected bool IsValid<TInternalEntity>(TInternalEntity entity, ValidationRule<TInternalEntity>[] validationRules, out string[] errors)
        {
            errors = validationRules.Where(x => !x.Rule(entity))
                .Select(x => x.ErrorMessage)
                .ToArray();

            return !errors.Any();
        }

        protected bool IsArrayValid<TInternalEntity>(TInternalEntity[] internalEntities, ValidationRule<TInternalEntity>[] validationRules, out string error)
        {
            var errors = new List<string>();
            for (int index = 0; index < internalEntities.Length; index++)
            {
                var internalEntity = internalEntities[index];

                if (!IsValid(internalEntity, validationRules, out var validationErrors))
                {
                    errors.Add($"{typeof(TEntity).Name} [{index + 1}] is invalid. Details:" + Environment.NewLine
                        + validationErrors.Select(x => "\t" + x).Aggregate((a, b) => a + Environment.NewLine + b));
                }
            }

            error = errors.Any()
                ? errors.Aggregate((a, b) => a + Environment.NewLine + b)
                : null;
            return !errors.Any();
        }
    }

    public class ValidationRule<T>
    {
        public Func<T, bool> Rule { get; set; }
        public string ErrorMessage { get; set; }
    }
}
