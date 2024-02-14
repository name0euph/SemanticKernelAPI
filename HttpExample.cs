using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SemanticKernelAPI
{
    public class HttpExample
    {
        private readonly ILogger<HttpExample> _logger;
        private readonly Kernel _kernel;

        public HttpExample(Kernel kernel, ILogger<HttpExample> logger)
        {
            _logger = logger;
            _kernel = kernel;
        }

        [Function("HttpExample")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            // ���N�G�X�g��Body�����o��
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Json�f�[�^����SummarizeRequest�I�u�W�F�N�g�Ƀf�V���A���C�Y
            var data = JsonConvert.DeserializeObject<SummarizeRequest>(requestBody);

            // �v��̃v�����v�g�e���v���[�g���쐬
            var prompt = @"{{$input}}
            �Ȃ�ׂ��Z���v�񂵂Ă��������B";

            // �v������s����֐����쐬
            var summarize = _kernel.CreateFunctionFromPrompt(
                promptTemplate: prompt,
                executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 200 }
                );

            try
            {
                // ���N�G�X�g�{�f�B���Ȃ��ꍇ�͗�O���X���[
                if (data == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                var result = await _kernel.InvokeAsync(summarize, new KernelArguments() { ["input"] = data.Text });

                Console.WriteLine(result);

                // �v�񌋉ʂ�Ԃ�
                return new OkObjectResult(result.GetValue<string>());
            }
            // ���N�G�X�g�{�f�B���Ȃ��ꍇ�̗�O����
            catch (ArgumentNullException e)
            {
                _logger.LogError(e, "���N�G�X�g�{�f�B������܂���B");
                return new OkObjectResult("���N�G�X�g�{�f�B������܂���B");
            }
        }
    }

        public class SummarizeRequest
    {
        public string Text { get; set; }
    }
}