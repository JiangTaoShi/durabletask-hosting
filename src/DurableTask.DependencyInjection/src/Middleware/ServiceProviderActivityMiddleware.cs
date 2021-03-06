﻿// Copyright (c) Jacob Viau. All rights reserved.
// Licensed under the APACHE 2.0. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using DurableTask.Core;
using DurableTask.Core.Middleware;
using DurableTask.DependencyInjection.Activities;
using DurableTask.DependencyInjection.Extensions;

namespace DurableTask.DependencyInjection.Middleware
{
    /// <summary>
    /// Middleware that constructs and injects the real orchestration or activity at the necessary time.
    /// </summary>
    public class ServiceProviderActivityMiddleware : ITaskMiddleware
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderActivityMiddleware"/> class.
        /// A middleware that lazily sets the inner orchestration to be ran.
        /// </summary>
        /// <param name="serviceProvider">The service provider. Not null.</param>
        public ServiceProviderActivityMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(DispatchMiddlewareContext context, Func<Task> next)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(next, nameof(next));

            TaskActivity taskActivity = context.GetProperty<TaskActivity>();

            if (taskActivity is WrapperActivity wrapper)
            {
                wrapper.InnerActivity = (TaskActivity)_serviceProvider
                    .GetServiceOrCreateInstance(wrapper.InnerActivityType);

                // update the context task activity with the real one.
                context.SetProperty(wrapper.InnerActivity);
                await next().ConfigureAwait(false);
                return;
            }

            await next().ConfigureAwait(false);
        }
    }
}
