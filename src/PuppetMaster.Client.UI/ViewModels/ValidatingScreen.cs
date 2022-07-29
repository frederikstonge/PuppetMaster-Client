using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace PuppetMaster.Client.UI.ViewModels
{
    public abstract class ValidatingScreen : Screen, INotifyDataErrorInfo
    {
        private IDictionary<string, List<string?>> _errors = new Dictionary<string, List<string?>>();
        private string? _globalErrorMessage;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors => _errors.Any();

        public bool HasGlobalErrorMessage => !string.IsNullOrEmpty(GlobalErrorMessage);

        public string? GlobalErrorMessage
        {
            get => _globalErrorMessage;
            set
            {
                _globalErrorMessage = value;
                NotifyOfPropertyChange(() => GlobalErrorMessage);
                NotifyOfPropertyChange(() => HasGlobalErrorMessage);
            }
        }

        public IEnumerable GetErrors(string? propertyName) =>
            propertyName != null && _errors.ContainsKey(propertyName)
            ? _errors[propertyName]
            : new List<string?>();

        public async Task ValidateAsync(Func<Task> action, string? errorMessage = null)
        {
            GlobalErrorMessage = null;
            try
            {
                await action.Invoke();
            }
            catch (Exception ex)
            {
                GlobalErrorMessage = string.IsNullOrEmpty(errorMessage) ? ex.Message : errorMessage;
            }
        }

        public void Validate(System.Action action, string? errorMessage = null)
        {
            GlobalErrorMessage = null;
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                GlobalErrorMessage = string.IsNullOrEmpty(errorMessage) ? ex.Message : errorMessage;
            }
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            PropertyChanged += ValidatingScreen_PropertyChanged;
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                PropertyChanged -= ValidatingScreen_PropertyChanged;
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected void Validate(object model)
        {
            _errors.Clear();
            ValidationContext context = new ValidationContext(model);
            List<ValidationResult> results = new ();
            if (!Validator.TryValidateObject(model, context, results))
            {
                foreach (var result in results)
                {
                    foreach (var memberName in result.MemberNames)
                    {
                        if (_errors.ContainsKey(memberName))
                        {
                            _errors[memberName] = _errors[memberName].Append(result.ErrorMessage).ToList();
                        }
                        else
                        {
                            _errors[memberName] = new List<string?>() { result.ErrorMessage };
                        }
                    }
                }
            }

            foreach (var error in _errors)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(error.Key));
            }
        }

        private static object? GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }

        private void ValidatingScreen_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Validate(this, e.PropertyName!, GetPropertyValue(this, e.PropertyName!));
        }

        private void Validate(object model, string propertyName, object? val)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
            }

            ValidationContext context = new ValidationContext(model)
            {
                MemberName = propertyName
            };

            List<ValidationResult> results = new ();

            if (!Validator.TryValidateProperty(val, context, results))
            {
                _errors[propertyName] = results.Select(x => x.ErrorMessage).ToList();
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
