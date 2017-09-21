using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace EfCustomizeSampleApp
{
    public class MyEntityTypeGenerator: CSharpEntityTypeGenerator
    {
        private ICSharpUtilities CSharpUtilities { get; }
        private IndentedStringBuilder _sb;
        private bool _useDataAnnotations;

        public MyEntityTypeGenerator(ICSharpUtilities cSharpUtilities)
            : base(cSharpUtilities)
        {
            CSharpUtilities = cSharpUtilities;
        }

        public override string WriteCode(IEntityType entityType, string @namespace, bool useDataAnnotations)
        {
            //return base.WriteCode(entityType, @namespace, useDataAnnotations);
            _sb = new IndentedStringBuilder();
            _useDataAnnotations = useDataAnnotations;

            _sb.AppendLine("using System;");
            _sb.AppendLine("using System.Collections.Generic;");
            _sb.AppendLine("using System.ComponentModel.DataAnnotations;");

            //if (_useDataAnnotations)
            //{
            //    _sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            //    _sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            //}

            foreach (var ns in entityType.GetProperties()
                .SelectMany(p => p.ClrType.GetNamespaces())
                .Where(ns => ns != "System" && ns != "System.Collections.Generic")
                .Distinct())
            {
                _sb.AppendLine($"using {ns};");
            }

            _sb.AppendLine();
            _sb.AppendLine($"namespace {@namespace}");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateClass(entityType);
            }

            _sb.AppendLine("}");

            return _sb.ToString();
        }

        private void GenerateClass(IEntityType entityType)
        {
            if (_useDataAnnotations)
            {
                GenerateEntityTypeDataAnnotations(entityType);
            }

            var tableName = entityType.Relational().TableName;
            var comment = CommentHelper.GetComment(tableName);
            if (!string.IsNullOrEmpty(comment))
            {
                _sb.AppendLine("/// <summary>");
                _sb.AppendLine($"/// {comment}");
                _sb.AppendLine("/// </summary>");
            }
            _sb.AppendLine($"public partial class {entityType.Name}");

            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateConstructor(entityType);
                GenerateProperties(entityType);
                GenerateNavigationProperties(entityType);
            }

            _sb.AppendLine("}");
        }

        private void GenerateEntityTypeDataAnnotations(IEntityType entityType)
        {
            GenerateTableAttribute(entityType);
        }

        private void GenerateTableAttribute(IEntityType entityType)
        {
            var tableName = entityType.Relational().TableName;
            var schema = entityType.Relational().Schema;
            var defaultSchema = entityType.Model.Relational().DefaultSchema;

            var schemaParameterNeeded = schema != null && schema != defaultSchema;
            var tableAttributeNeeded = schemaParameterNeeded || tableName != null && tableName != entityType.Scaffolding().DbSetName;

            if (tableAttributeNeeded)
            {
                var tableAttribute = new AttributeWriter(nameof(TableAttribute));

                tableAttribute.AddParameter(CSharpUtilities.DelimitString(tableName));

                if (schemaParameterNeeded)
                {
                    tableAttribute.AddParameter($"{nameof(TableAttribute.Schema)} = {CSharpUtilities.DelimitString(schema)}");
                }

                _sb.AppendLine(tableAttribute.ToString());
            }
        }

        private void GenerateConstructor(IEntityType entityType)
        {
            var collectionNavigations = entityType.GetNavigations().Where(n => n.IsCollection()).ToList();

            if (collectionNavigations.Count > 0)
            {
                _sb.AppendLine($"public {entityType.Name}()");
                _sb.AppendLine("{");

                using (_sb.Indent())
                {
                    foreach (var navigation in collectionNavigations)
                    {
                        _sb.AppendLine($"{navigation.Name} = new HashSet<{navigation.GetTargetType().Name}>();");
                    }
                }

                _sb.AppendLine("}");
                _sb.AppendLine();
            }
        }

        private void GenerateProperties(IEntityType entityType)
        {
            var comments = CommentHelper.GetComments(entityType.Relational().TableName);
            //occurs exception
            //var properies = entityType.GetProperties()
            //    .OrderBy(p => ScaffoldingMetadataExtensions.Scaffolding(p).ColumnOrdinal);
            var properties = entityType.GetProperties().OrderBy(p => new ScaffoldingPropertyAnnotations(p).ColumnOrdinal);
            foreach (var property in properties)
            {
                if (_useDataAnnotations)
                {
                    GeneratePropertyDataAnnotations(property);
                }
                if (comments.Count > 0)
                {
                    var columnName = property.Relational().ColumnName;
                    if (comments.ContainsKey(columnName))
                    {
                        var comment = comments[columnName];
                        _sb.AppendLine("/// <summary>");
                        _sb.AppendLine($"/// {comment}");
                        _sb.AppendLine("/// </summary>");
                        _sb.AppendLine($"[Display(Name = \"{comment}\")]");
                    }
                }

                _sb.AppendLine($"public {CSharpUtilities.GetTypeName(property.ClrType)} {property.Name} {{ get; set; }}");
            }
        }

        private void GeneratePropertyDataAnnotations(IProperty property)
        {
            GenerateKeyAttribute(property);
            GenerateRequiredAttribute(property);
            GenerateColumnAttribute(property);
            GenerateMaxLengthAttribute(property);
        }

        private void GenerateKeyAttribute(IProperty property)
        {
            var key = property.AsProperty().PrimaryKey;

            if (key?.Properties.Count == 1)
            {
                if (key is Key concreteKey
                    && key.Properties.SequenceEqual(new KeyDiscoveryConvention().DiscoverKeyProperties(concreteKey.DeclaringEntityType, concreteKey.DeclaringEntityType.GetProperties().ToList())))
                {
                    return;
                }

                if (key.Relational().Name != ConstraintNamer.GetDefaultName(key))
                {
                    return;
                }

                _sb.AppendLine(new AttributeWriter(nameof(KeyAttribute)));
            }
        }

        private void GenerateColumnAttribute(IProperty property)
        {
            var columnName = property.Relational().ColumnName;
            var columnType = property.Relational().ColumnType;

            var delimitedColumnName = columnName != null && columnName != property.Name ? CSharpUtilities.DelimitString(columnName) : null;
            var delimitedColumnType = columnType != null ? CSharpUtilities.DelimitString(columnType) : null;

            if ((delimitedColumnName ?? delimitedColumnType) != null)
            {
                var columnAttribute = new AttributeWriter(nameof(ColumnAttribute));

                if (delimitedColumnName != null)
                {
                    columnAttribute.AddParameter(delimitedColumnName);
                }

                if (delimitedColumnType != null)
                {
                    columnAttribute.AddParameter($"{nameof(ColumnAttribute.TypeName)} = {delimitedColumnType}");
                }

                _sb.AppendLine(columnAttribute);
            }
        }

        private void GenerateMaxLengthAttribute(IProperty property)
        {
            var maxLength = property.GetMaxLength();

            if (maxLength.HasValue)
            {
                var lengthAttribute = new AttributeWriter(
                    property.ClrType == typeof(string)
                        ? nameof(StringLengthAttribute)
                        : nameof(MaxLengthAttribute));

                lengthAttribute.AddParameter(CSharpUtilities.GenerateLiteral(maxLength.Value));

                _sb.AppendLine(lengthAttribute.ToString());
            }
        }

        private void GenerateRequiredAttribute(IProperty property)
        {
            //IsNullableType can't access
            //if (!property.IsNullable
            //    && property.ClrType.IsNullableType()
            //    && !property.IsPrimaryKey())
            //{
            //    _sb.AppendLine(new AttributeWriter(nameof(RequiredAttribute)).ToString());
            //}
            if (!property.IsNullable
                && IsNullableType(property.ClrType)
                && !property.IsPrimaryKey())
            {
                _sb.AppendLine(new AttributeWriter(nameof(RequiredAttribute)).ToString());
            }
        }

        private void GenerateNavigationProperties(IEntityType entityType)
        {
            var sortedNavigations = entityType.GetNavigations()
                .OrderBy(n => n.IsDependentToPrincipal() ? 0 : 1)
                .ThenBy(n => n.IsCollection() ? 1 : 0);

            if (sortedNavigations.Any())
            {
                _sb.AppendLine();

                foreach (var navigation in sortedNavigations)
                {
                    if (_useDataAnnotations)
                    {
                        GenerateNavigationDataAnnotations(navigation);
                    }

                    var referencedTypeName = navigation.GetTargetType().Name;
                    var navigationType = navigation.IsCollection() ? $"ICollection<{referencedTypeName}>" : referencedTypeName;
                    _sb.AppendLine($"public {navigationType} {navigation.Name} {{ get; set; }}");
                }
            }
        }

        private void GenerateNavigationDataAnnotations(INavigation navigation)
        {
            GenerateForeignKeyAttribute(navigation);
            GenerateInversePropertyAttribute(navigation);
        }

        private void GenerateForeignKeyAttribute(INavigation navigation)
        {
            if (navigation.IsDependentToPrincipal())
            {
                if (navigation.ForeignKey.PrincipalKey.IsPrimaryKey())
                {
                    var foreignKeyAttribute = new AttributeWriter(nameof(ForeignKeyAttribute));

                    foreignKeyAttribute.AddParameter(
                        CSharpUtilities.DelimitString(
                            string.Join(",", navigation.ForeignKey.Properties.Select(p => p.Name))));

                    _sb.AppendLine(foreignKeyAttribute.ToString());
                }
            }
        }

        private void GenerateInversePropertyAttribute(INavigation navigation)
        {
            if (navigation.ForeignKey.PrincipalKey.IsPrimaryKey())
            {
                var inverseNavigation = navigation.FindInverse();

                if (inverseNavigation != null)
                {
                    var inversePropertyAttribute = new AttributeWriter(nameof(InversePropertyAttribute));

                    inversePropertyAttribute.AddParameter(CSharpUtilities.DelimitString(inverseNavigation.Name));

                    _sb.AppendLine(inversePropertyAttribute.ToString());
                }
            }
        }

        private class AttributeWriter
        {
            private readonly string _attibuteName;
            private readonly List<string> _parameters = new List<string>();

            public AttributeWriter(string attributeName)
            {

                _attibuteName = attributeName;
            }

            public void AddParameter(string parameter)
            {

                _parameters.Add(parameter);
            }

            public override string ToString()
                => "[" + (_parameters.Count == 0
                       ? StripAttribute(_attibuteName)
                       : StripAttribute(_attibuteName) + "(" + string.Join(", ", _parameters) + ")") + "]";

            private static string StripAttribute(string attributeName)
                => attributeName.EndsWith("Attribute", StringComparison.Ordinal)
                    ? attributeName.Substring(0, attributeName.Length - 9)
                    : attributeName;
        }

        //Copy From SharedTypeExtensions
        private bool IsNullableType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return !typeInfo.IsValueType
                   || typeInfo.IsGenericType
                   && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

    }
}
