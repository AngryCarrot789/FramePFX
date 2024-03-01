# Live Templates
## Automatable Parameters

```
public static readonly Parameter$paramType$ $propNameField$Parameter = 
    Parameter.Register$paramType$(
        typeof($currentclass$), 
        nameof($currentclass$), 
        nameof($propNameField$), default($clrType$), 
        ValueAccessors.Reflective<$clrType$>(typeof($currentclass$), nameof($propNameField$)), 
        ParameterFlags.StandardProjectVisual);

private $clrType$ $propNameField$ = $propNameField$Parameter.Descriptor.DefaultValue;
```

## Data Parameters

### Public Getter/Setter
```
public static readonly DataParameter$dataParamType$ $propName$Parameter = 
    DataParameter.Register(
        new DataParameter$dataParamType$(
            typeof($currentclass$), 
            nameof($propName$), default($clrType$), 
            ValueAccessors.Reflective<$clrType$>(typeof($currentclass$), nameof($backingField$)), 
            DataParameterFlags.StandardProjectVisual));

private $clrType$ $backingField$ = $propName$Parameter.DefaultValue;

public $clrType$ $propName$ {
    get => this.$backingField$;
    set => DataParameter.SetValueHelper(this, $propName$Parameter, ref this.$backingField$, value);
}
```

### No Getter/Setter, just backing field

```
public static readonly DataParameter$dataParamType$ $propName$Parameter = 
    DataParameter.Register(
        new DataParameter$dataParamType$(
            typeof($currentclass$), 
            nameof($propName$), default($clrType$), 
            ValueAccessors.Reflective<$clrType$>(typeof($currentclass$), nameof($propName$)), 
            DataParameterFlags.StandardProjectVisual));

private $clrType$ $propName$ = $propName$Parameter.DefaultValue;
```
