# aws_api_gateway_model.tfer--wd2r72:
resource "aws_api_gateway_model" "tfer--wd2r72" {
    content_type = "application/json"
    description  = "This is a default empty schema model"
    id           = "wd2r72"
    name         = "Empty"
    rest_api_id  = "uwetoiwgxd"
    schema       = jsonencode(
        {
            "$schema" = "http://json-schema.org/draft-04/schema#"
            title     = "Empty Schema"
            type      = "object"
        }
    )
}
