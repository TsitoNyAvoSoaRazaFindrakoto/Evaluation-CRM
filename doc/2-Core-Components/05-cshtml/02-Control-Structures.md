# Control Structures in Razor

## Loops

### foreach Loop
```cshtml
@foreach (var item in Model.Items)
{
    <div class="item">
        <h3>@item.Name</h3>
        <p>@item.Description</p>
    </div>
}
```

### for Loop
```cshtml
@for (int i = 0; i < Model.Count; i++)
{
    <div>Item @i: @Model[i].Name</div>
}
```

### while Loop
```cshtml
@{int i = 0;}
@while (i < 5)
{
    <p>Line @i</p>
    i++;
}
```

## Conditional Statements

### if Statement
```cshtml
@if (Model.IsAdmin)
{
    <div>Admin Panel</div>
}
```

### if-else Statement
```cshtml
@if (Model.IsAuthenticated)
{
    <div>Welcome back!</div>
}
else
{
    <div>Please log in</div>
}
```

### switch Statement
```cshtml
@switch (Model.UserType)
{
    case "Admin":
        <div>Admin Panel</div>
        break;
    case "User":
        <div>User Dashboard</div>
        break;
    default:
        <div>Guest View</div>
        break;
}
```
