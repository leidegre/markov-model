
# Example: build & generate

```cs
var mc = new MarkovChain(3); // k = degree

mc.Add("a");
mc.Add("as");
mc.Add("asd");
mc.Add("asd1");
mc.Add("asd12");
mc.Add("asd123");

// compute distribution:
//  required before calling `Generate`
mc.Prepare(); 

var r = new Random(3);

// generate value
mc.Generate(r)

// generate value in string builder
var sb = new StringBuilder();
mc.Generate(r, sb);

// save to disk
mc.WriteToFile(...);
```

# Example: load from file

```cs
var mc = new MarkovChain(); // note: parameterless constructor

// load from disk
mc.ReadFromFile(...);

// no need to call `Prepare`

// generate value
mc.Generate(r)

// generate value in string builder
var sb = new StringBuilder();
mc.Generate(r, sb);
```