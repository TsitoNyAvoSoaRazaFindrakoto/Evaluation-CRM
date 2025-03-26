# Razor Expressions

## Basic Syntax

### Variable Declaration and Usage
```cshtml
@{
    var message = "Hello World";
    var number = 42;
}
<p>@message</p>
<p>@(number * 2)</p>
```

### Model Access
```cshtml
@model UserViewModel

<h1>Welcome, @Model.Name!</h1>
<p>Email: @Model.Email</p>
```

### ViewData and ViewBag
```cshtml
@ViewData["Title"]
@ViewBag.Message
```

### Raw HTML Output
```cshtml
@Html.Raw("<strong>Bold Text</strong>")
```

### Escaping @ Symbol
```cshtml
<p>Email us at user@@domain.com</p>
@@Model.Property
```

### Comments
```cshtml
@* This is a Razor comment *@
<!-- HTML comment -->
```
