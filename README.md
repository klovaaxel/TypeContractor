# TypeContractor

Looks at one or more assemblies containing contracts and translates to TypeScript interfaces and enums

## Goals

1. Take one or more assemblies and reflect to find types matching a list
   of suffixes (by default `Dto`, `Request`, and `Response`). If a type doesn't
   match the suffix, it gets ignored -- unless a type that *is* matched
   has this type as a dependency. In that case, we have to have the type
   even if the suffix doesn't match.

   For example:

   We have a type `GetPaymentListResponse` that gets included from the default
   list of suffixes. That type has a list of `GetPaymentListResponse_Payment`,
   which would be ignored by configuration, but since it is a part of 
   `GetPaymentListResponse`, we convert both.

   `GetPaymentListResponse_Payment` also has a reference to an enum,
   `PaymentState`, that gets ignored by the configuration. But again, since it
   is a dependency, it gets converted.

2. Perform replacements on the names to strip away prefixes

   `Hogia.SalaryService.Common` is unnecessary to have in the output path.
   We can strip the common prefix and get `api/Modules/Absence/AbsenceRequest.ts`
   instead of `api/Hogia/SalaryService/Common/Modules/Absence/AbsenceRequest.ts`.

3. Map custom types to their TypeScript-friendly counterparts if necessary.

   For example, SalaryService has a custom `Money` type that maps down to
   `number`. If we don't configure that manually, it will create the `Money`
   interface, which only contains `amount` as a `number`. That's both
   cumbersome to work with, as well as *wrong*, since the BFF will 
   serialize `Money` as a `number`.


## Setup and configuration

To accomplish the same configuration and results as described under Goals,
create Contractor like this:

```csharp
Contractor.FromDefaultConfiguration(configuration => configuration
            .AddAssembly("Hogia.SalaryService.Common", "Hogia.SalaryService.Common.dll")
            .AddCustomMap("Hogia.SalaryService.Common.Types.Money", DestinationTypes.Number)
            .StripString("Hogia.SalaryService.Common")
            .SetOutputDirectory(Path.Combine(Directory.GetCurrentDirectory(), "api")));
```

## Run manually

Get an instance of `Contractor` and call `contractor.Build();`

## Run automatically

Improvements needed

## Future improvements

* Kebab-case output files and directories
* Better documentation
* Better performance -- if this should run on build, it can't take forever
* Run automatically
* Install as a global dotnet tool?
