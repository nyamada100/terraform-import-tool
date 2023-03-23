# aws_api_gateway_integration_response.tfer--m72beguo05-002F-orjnp5-002F-PUT-002F-200:
resource "aws_api_gateway_integration_response" "tfer--m72beguo05-002F-orjnp5-002F-PUT-002F-200" {
    http_method         = "PUT"
    id                  = "m72beguo05/orjnp5/PUT/200"
    resource_id         = "orjnp5"
    response_parameters = {
        "method.response.header.Access-Control-Allow-Origin" = "'*'"
    }
    response_templates  = {
        "application/json" = ""
    }
    rest_api_id         = "m72beguo05"
    status_code         = "200"
}
