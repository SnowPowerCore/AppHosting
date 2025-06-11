using AppHosting.Xamarin.Forms.Abstractions.Delegates;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Middleware;
using AppHosting.Xamarin.Forms.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace AppHosting.Xamarin.Forms.Middleware
{
    public class ChildrenBindingContextMiddleware : IElementMiddleware
    {
        private readonly IServiceProvider _services;

        public ChildrenBindingContextMiddleware(IServiceProvider services)
        {
            _services = services;
        }

        public Task InvokeAsync(Element element, ElementDelegate next)
        {
            var bindingContextAttrs = element.GetElementBindingContextAttributes();
            var xfElementType = element.GetType();

            var childrenBindingContexts = bindingContextAttrs
                .Where(x => !string.IsNullOrEmpty(x.ControlName));

            foreach (var childrenBindingContext in childrenBindingContexts)
            {
                // Try to use property first (preferred in MAUI)
                var property = xfElementType.GetProperty(childrenBindingContext.ControlName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null && typeof(BindableObject).IsAssignableFrom(property.PropertyType))
                {
                    var bindableObj = property.GetValue(element) as BindableObject;
                    if (bindableObj != null)
                        bindableObj.BindingContext = _services.GetService(childrenBindingContext.BindingContextType);
                    continue;
                }
                // Fallback to field (legacy pattern)
                var field = xfElementType.GetField(childrenBindingContext.ControlName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null && typeof(BindableObject).IsAssignableFrom(field.FieldType))
                {
                    var bindableObj = field.GetValue(element) as BindableObject;
                    if (bindableObj != null)
                        bindableObj.BindingContext = _services.GetService(childrenBindingContext.BindingContextType);
                }
            }
            return next(element);
        }
    }
}