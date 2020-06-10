module App.Common.Validators

type ValidationResult<'T> = Result<'T, string>
type Validation<'T> = 'T->ValidationResult<'T>

let nonEmptyString name (x:string): ValidationResult<string> =
    match x with
    | x when System.String.IsNullOrEmpty x -> sprintf "Value is null: %s" name |> Error
    | _ -> Ok x

let toGuid (s:string): ValidationResult<System.Guid> = 
    match System.Guid.TryParse(s) with
    |true, id -> id |> Ok
    |false, _ -> sprintf "not a valid guid: %s" s |> Error
