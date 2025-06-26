﻿using AppHosting.Xamarin.Forms.Abstractions.Delegates;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Builders;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Factory;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Processors;
using System;

namespace AppHosting.Xamarin.Forms.Services.Processors
{
    public class AppVisualProcessor : IAppVisualProcessor
    {
        public PageDelegate PageProcessing { get; }

        public ElementDelegate ElementProcessing { get; }

        public AppVisualProcessor(IPageBuilderFactory pageBuilderFactory,
                                  IElementBuilderFactory elementBuilderFactory,
                                  IPageElementConfigure pageElementConfigure)
        {
            var pageBuilder = pageBuilderFactory.CreatePageBuilder();
            var elementBuilder = elementBuilderFactory.CreateElementBuilder();

            Action<IPageBuilder> configurePage = pageElementConfigure.ConfigurePage;
            configurePage(pageBuilder);

            Action<IElementBuilder> configureElement = pageElementConfigure.ConfigureElement;
            configureElement(elementBuilder);

            PageProcessing = pageBuilder.Build();
            ElementProcessing = elementBuilder.Build();
        }
    }
}