using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Vargshala.Web.Components.UI.Validation;

public class FluentValidationValidator : ComponentBase, IDisposable
{
    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    [Parameter]
    public IValidator? Validator { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    private ValidationMessageStore? _messageStore;

    protected override void OnInitialized()
    {
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException($"{nameof(FluentValidationValidator)} requires a cascading " +
                $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(FluentValidationValidator)} " +
                $"inside an {nameof(EditForm)}.");
        }

        _messageStore = new ValidationMessageStore(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += HandleValidationRequested;
        CurrentEditContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        if (CurrentEditContext == null || _messageStore == null) return;

        _messageStore.Clear();

        var validator = GetValidator();
        if (validator == null) return;

        var context = new ValidationContext<object>(CurrentEditContext.Model);
        var validationResult = validator.Validate(context);

        foreach (var errorGroup in validationResult.Errors.GroupBy(e => e.PropertyName))
        {
            var firstError = errorGroup.First();
            var fieldIdentifier = new FieldIdentifier(CurrentEditContext.Model, firstError.PropertyName);
            _messageStore.Add(fieldIdentifier, firstError.ErrorMessage);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (CurrentEditContext == null || _messageStore == null) return;

        _messageStore.Clear(args.FieldIdentifier);

        var validator = GetValidator();
        if (validator == null) return;

        var context = new ValidationContext<object>(
            CurrentEditContext.Model,
            new PropertyChain(),
            new MemberNameValidatorSelector(new[] { args.FieldIdentifier.FieldName }));

        var validationResult = validator.Validate(context);

        var firstError = validationResult.Errors.FirstOrDefault();
        if (firstError != null)
        {
            _messageStore.Add(args.FieldIdentifier, firstError.ErrorMessage);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    private IValidator? GetValidator()
    {
        if (Validator != null) return Validator;

        if (CurrentEditContext == null) return null;

        var modelType = CurrentEditContext.Model.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(modelType);

        var validator = ServiceProvider.GetService(validatorType) as IValidator;
        if (validator != null) return validator;

        // Fallback: check if standard matching validator exists in assembly
        var directTypeName = $"{modelType.FullName}Validator";
        var directType = modelType.Assembly.GetType(directTypeName);
        if (directType != null && typeof(IValidator).IsAssignableFrom(directType))
        {
            return (IValidator)Activator.CreateInstance(directType)!;
        }

        return null;
    }

    public void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationRequested -= HandleValidationRequested;
            CurrentEditContext.OnFieldChanged -= HandleFieldChanged;
        }
    }
}
