# aws_api_gateway_integration.tfer--uwetoiwgxd-002F-le78yt-002F-POST:
resource "aws_api_gateway_integration" "tfer--uwetoiwgxd-002F-le78yt-002F-POST" {
    cache_key_parameters    = []
    cache_namespace         = "le78yt"
    connection_type         = "INTERNET"
    http_method             = "POST"
    id                      = "uwetoiwgxd/le78yt/POST"
    integration_http_method = "POST"
    passthrough_behavior    = "WHEN_NO_MATCH"
    request_parameters      = {}
    request_templates       = {}
    resource_id             = "le78yt"
    rest_api_id             = "uwetoiwgxd"
    timeout_milliseconds    = 29000
    type                    = "AWS_PROXY"
    uri                     = "arn:aws:apigateway:ap-northeast-1:lambda:path/2015-03-31/functions/arn:aws:lambda:ap-northeast-1:498892324186:function:storeci-dev-backlog-webhook-lambda/invocations"
}
