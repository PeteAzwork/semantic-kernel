﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

/// <summary>
/// OpenAI chat completion client.
/// </summary>
public sealed class OpenAIChatCompletion : IChatCompletion, ITextCompletion
{
    private readonly OpenAIClientCore _core;

    /// <summary>
    /// Create an instance of the OpenAI chat completion connector
    /// </summary>
    /// <param name="modelId">Model name</param>
    /// <param name="apiKey">OpenAI API Key</param>
    /// <param name="organization">OpenAI Organization Id (usually optional)</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    public OpenAIChatCompletion(
        string modelId,
        string apiKey,
        string? organization = null,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        this._core = new(modelId, apiKey, organization, httpClient, loggerFactory?.CreateLogger(typeof(OpenAIChatCompletion)));

        this._core.AddAttribute(AIServiceExtensions.ModelIdKey, modelId);
        this._core.AddAttribute(OpenAIClientCore.OrganizationKey, organization);
    }

    /// <summary>
    /// Create an instance of the OpenAI chat completion connector
    /// </summary>
    /// <param name="modelId">Model name</param>
    /// <param name="openAIClient">Custom <see cref="OpenAIClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    public OpenAIChatCompletion(
        string modelId,
        OpenAIClient openAIClient,
        ILoggerFactory? loggerFactory = null)
    {
        this._core = new(modelId, openAIClient, loggerFactory?.CreateLogger(typeof(OpenAIChatCompletion)));

        this._core.AddAttribute(AIServiceExtensions.ModelIdKey, modelId);
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Attributes => this.Attributes;

    /// <inheritdoc/>
    public ChatHistory CreateNewChat(string? instructions = null) =>
        OpenAIClientCore.CreateNewChat(instructions);

    /// <inheritdoc/>
    public Task<IReadOnlyList<IChatResult>> GetChatCompletionsAsync(
        ChatHistory chat,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        this._core.LogActionDetails();

        return this._core.GetChatResultsAsync(chat, executionSettings, kernel, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<IChatResult>> GetChatCompletionsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        Verify.NotNullOrWhiteSpace(prompt);

        this._core.LogActionDetails();

        var openAIExecutionSettings = OpenAIPromptExecutionSettings.FromExecutionSettings(executionSettings);
        var chatHistory = ClientCore.CreateNewChat(prompt, openAIExecutionSettings);

        return this._core.GetChatResultsAsync(chatHistory, executionSettings, kernel, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ITextResult>> GetCompletionsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        Verify.NotNullOrWhiteSpace(prompt);

        this._core.LogActionDetails();

        return this._core.GetChatResultsAsTextAsync(prompt, executionSettings, kernel, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<T> GetStreamingContentAsync<T>(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        Verify.NotNullOrWhiteSpace(prompt);

        this._core.LogActionDetails();

        var openAIExecutionSettings = OpenAIPromptExecutionSettings.FromExecutionSettings(executionSettings);
        var chatHistory = ClientCore.CreateNewChat(prompt, openAIExecutionSettings);

        return this._core.GetChatStreamingUpdatesAsync<T>(chatHistory, openAIExecutionSettings, kernel, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<T> GetStreamingContentAsync<T>(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        this._core.LogActionDetails();

        return this._core.GetChatStreamingUpdatesAsync<T>(chatHistory, executionSettings, kernel, cancellationToken);
    }
}
