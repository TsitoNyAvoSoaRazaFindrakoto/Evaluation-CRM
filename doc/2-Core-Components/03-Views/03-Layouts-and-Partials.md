# ASP.NET MVC Layouts and Partial Views

## Layout Views

Layouts provide a consistent template for your application.

### Layout Definition
```cshtml
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"]</title>
</head>
<body>
    <header>@await Html.PartialAsync("_Header")</header>
    <main>
        @RenderBody()
    </main>
    <footer>@await Html.PartialAsync("_Footer")</footer>
    @RenderSection("Scripts", required: false)
</body>
</html>
```

### Using Layouts
```cshtml
@{
    Layout = "_Layout";
}
```

## Partial Views

Partial views are reusable view segments.

### Creating Partials
```cshtml
@model UserProfileViewModel
<div class="profile-card">
    <h3>@Model.Name</h3>
    <p>@Model.Email</p>
</div>
```

### Using Partials
```cshtml
@await Html.PartialAsync("_UserProfile", Model.UserProfile)
@Html.Partial("_UserProfile", Model.UserProfile)
```
