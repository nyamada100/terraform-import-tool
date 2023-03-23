# aws_api_gateway_model.tfer--49h3pf:
resource "aws_api_gateway_model" "tfer--49h3pf" {
    content_type = "application/json"
    description  = "This is a default error schema model"
    id           = "49h3pf"
    name         = "Error"
    rest_api_id  = "uwetoiwgxd"
    schema       = jsonencode(
        {
            "$schema"  = "http://json-schema.org/draft-04/schema#"
            properties = {
                message = {
                    type = "string"
                }
            }
            title      = "Error Schema"
            type       = "object"
        }
    )
}
