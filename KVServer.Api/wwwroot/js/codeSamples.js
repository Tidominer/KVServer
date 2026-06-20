// Code samples for different programming languages and actions
class CodeSamples {
    constructor() {
        this.samples = {
            javascript: {
                read: `// Read a key value
const token = 'YOUR_ACCESS_TOKEN';
const keyName = 'my_key';

fetch(\`/api/keys/\${keyName}\`, {
    headers: {
        'X-Access-Token': token
    }
})
.then(response => response.json())
.then(data => {
    console.log('Value:', data.value);
    console.log('Version:', data.version);
})
.catch(error => console.error('Error:', error));`,

                create: `// Create a new key-value pair
const token = 'YOUR_ACCESS_TOKEN';
const keyName = 'my_key';
const value = 'my_value';

fetch('/api/keys', {
    method: 'POST',
    headers: {
        'X-Access-Token': token,
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        key: keyName,
        value: value
    })
})
.then(response => response.json())
.then(data => {
    console.log('Key created successfully!');
    console.log('Key:', data.key);
    console.log('Version:', data.version);
})
.catch(error => console.error('Error:', error));`,

                update: `// Update an existing key
const token = 'YOUR_ACCESS_TOKEN';
const keyName = 'my_key';
const newValue = 'updated_value';

fetch(\`/api/keys/\${keyName}\`, {
    method: 'PUT',
    headers: {
        'X-Access-Token': token,
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        value: newValue
    })
})
.then(response => response.json())
.then(data => {
    console.log('Key updated successfully!');
    console.log('New version:', data.version);
})
.catch(error => console.error('Error:', error));`,

                list: `// List all keys in storage
const token = 'YOUR_ACCESS_TOKEN';

fetch('/api/keys', {
    headers: {
        'X-Access-Token': token
    }
})
.then(response => response.json())
.then(data => {
    console.log('Keys:', data.keys);
    data.keys.forEach(key => {
        console.log(\`- \${key.key} (v\${key.version})\`);
    });
})
.catch(error => console.error('Error:', error));`,

                delete: `// Delete a key
const token = 'YOUR_ACCESS_TOKEN';
const keyName = 'my_key';

fetch(\`/api/keys/\${keyName}\`, {
    method: 'DELETE',
    headers: {
        'X-Access-Token': token
    }
})
.then(response => {
    if (response.ok) {
        console.log('Key deleted successfully!');
    }
})
.catch(error => console.error('Error:', error));`,

                history: `// Get version history of a key
const token = 'YOUR_ACCESS_TOKEN';
const keyName = 'my_key';

fetch(\`/api/keys/\${keyName}/history\`, {
    headers: {
        'X-Access-Token': token
    }
})
.then(response => response.json())
.then(data => {
    console.log('Version history:', data.history);
    data.history.forEach(version => {
        console.log(\`v\${version.version}: \${version.value}\`);
        console.log(\`  Created: \${version.createdAt}\`);
    });
})
.catch(error => console.error('Error:', error));`
            },

            python: {
                read: `import requests

# Read a key value
token = 'YOUR_ACCESS_TOKEN'
key_name = 'my_key'

headers = {
    'X-Access-Token': token
}

response = requests.get(f'http://localhost:8080/api/keys/{key_name}', headers=headers)
data = response.json()

print(f'Value: {data["value"]}')
print(f'Version: {data["version"]}')`,

                create: `import requests

# Create a new key-value pair
token = 'YOUR_ACCESS_TOKEN'
key_name = 'my_key'
value = 'my_value'

headers = {
    'X-Access-Token': token,
    'Content-Type': 'application/json'
}

data = {
    'key': key_name,
    'value': value
}

response = requests.post('http://localhost:8080/api/keys', headers=headers, json=data)
result = response.json()

print(f'Key created successfully!')
print(f'Key: {result["key"]}')
print(f'Version: {result["version"]}')`,

                update: `import requests

# Update an existing key
token = 'YOUR_ACCESS_TOKEN'
key_name = 'my_key'
new_value = 'updated_value'

headers = {
    'X-Access-Token': token,
    'Content-Type': 'application/json'
}

data = {
    'value': new_value
}

response = requests.put(f'http://localhost:8080/api/keys/{key_name}', headers=headers, json=data)
result = response.json()

print(f'Key updated successfully!')
print(f'New version: {result["version"]}')`,

                list: `import requests

# List all keys in storage
token = 'YOUR_ACCESS_TOKEN'

headers = {
    'X-Access-Token': token
}

response = requests.get('http://localhost:8080/api/keys', headers=headers)
data = response.json()

print('Keys:')
for key in data['keys']:
    print(f'- {key["key"]} (v{key["version"]})')`,

                delete: `import requests

# Delete a key
token = 'YOUR_ACCESS_TOKEN'
key_name = 'my_key'

headers = {
    'X-Access-Token': token
}

response = requests.delete(f'http://localhost:8080/api/keys/{key_name}', headers=headers)

if response.ok:
    print('Key deleted successfully!')`,

                history: `import requests

# Get version history of a key
token = 'YOUR_ACCESS_TOKEN'
key_name = 'my_key'

headers = {
    'X-Access-Token': token
}

response = requests.get(f'http://localhost:8080/api/keys/{key_name}/history', headers=headers)
data = response.json()

print('Version history:')
for version in data['history']:
    print(f'v{version["version"]}: {version["value"]}')
    print(f'  Created: {version["createdAt"]}')`
            },

            curl: {
                read: `# Read a key value
TOKEN="YOUR_ACCESS_TOKEN"
KEY_NAME="my_key"

curl -X GET \\
  http://localhost:8080/api/keys/$KEY_NAME \\
  -H "X-Access-Token: $TOKEN"`,

                create: `# Create a new key-value pair
TOKEN="YOUR_ACCESS_TOKEN"
KEY_NAME="my_key"
VALUE="my_value"

curl -X POST http://localhost:8080/api/keys \\
  -H "X-Access-Token: $TOKEN" \\
  -H "Content-Type: application/json" \\
  -d '{"key":"'$KEY_NAME'","value":"'$VALUE'"}'`,

                update: `# Update an existing key
TOKEN="YOUR_ACCESS_TOKEN"
KEY_NAME="my_key"
NEW_VALUE="updated_value"

curl -X PUT \\
  http://localhost:8080/api/keys/$KEY_NAME \\
  -H "X-Access-Token: $TOKEN" \\
  -H "Content-Type: application/json" \\
  -d '{"value":"'$NEW_VALUE'"}'`,

                list: `# List all keys in storage
TOKEN="YOUR_ACCESS_TOKEN"

curl -X GET http://localhost:8080/api/keys \\
  -H "X-Access-Token: $TOKEN"`,

                delete: `# Delete a key
TOKEN="YOUR_ACCESS_TOKEN"
KEY_NAME="my_key"

curl -X DELETE \\
  http://localhost:8080/api/keys/$KEY_NAME \\
  -H "X-Access-Token: $TOKEN"`,

                history: `# Get version history of a key
TOKEN="YOUR_ACCESS_TOKEN"
KEY_NAME="my_key"

curl -X GET \\
  http://localhost:8080/api/keys/$KEY_NAME/history \\
  -H "X-Access-Token: $TOKEN"`
            },

            csharp: {
                read: `using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class KVClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8080/api";

    public KVClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Access-Token", token);
    }

    // Read a key value
    public async Task<(string Value, int Version)> GetKeyAsync(string keyName)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/keys/{keyName}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(content);

        return (data.GetProperty("value").GetString(),
                data.GetProperty("version").GetInt32());
    }
}

// Usage
var client = new KVClient("YOUR_ACCESS_TOKEN");
var (value, version) = await client.GetKeyAsync("my_key");
Console.WriteLine($"Value: {value}");
Console.WriteLine($"Version: {version}");`,

                create: `using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class KVClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8080/api";

    public KVClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Access-Token", token);
    }

    // Create a new key-value pair
    public async Task CreateKeyAsync(string keyName, string value)
    {
        var payload = new { key = keyName, value = value };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}/keys", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Console.WriteLine($"Key created successfully!");
        Console.WriteLine($"Key: {data.GetProperty("key").GetString()}");
        Console.WriteLine($"Version: {data.GetProperty("version").GetInt32()}");
    }
}

// Usage
var client = new KVClient("YOUR_ACCESS_TOKEN");
await client.CreateKeyAsync("my_key", "my_value");`,

                update: `using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class KVClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8080/api";

    public KVClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Access-Token", token);
    }

    // Update an existing key
    public async Task UpdateKeyAsync(string keyName, string newValue)
    {
        var payload = new { value = newValue };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"{BaseUrl}/keys/{keyName}", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Console.WriteLine($"Key updated successfully!");
        Console.WriteLine($"New version: {data.GetProperty("version").GetInt32()}");
    }
}

// Usage
var client = new KVClient("YOUR_ACCESS_TOKEN");
await client.UpdateKeyAsync("my_key", "updated_value");`,

                list: `using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class KVClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8080/api";

    public KVClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Access-Token", token);
    }

    // List all keys in storage
    public async Task<ListKeysResponse> GetKeysAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/keys");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ListKeysResponse>(content);
    }
}

// Usage
var client = new KVClient("YOUR_ACCESS_TOKEN");
var result = await client.GetKeysAsync();

Console.WriteLine("Keys:");
foreach (var key in result.Keys)
{
    Console.WriteLine($"- {key.Key} (v{key.Version})");
}`,

                delete: `using System.Net.Http;
using System.Threading.Tasks;

class KVClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8080/api";

    public KVClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Access-Token", token);
    }

    // Delete a key
    public async Task DeleteKeyAsync(string keyName)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/keys/{keyName}");
        response.EnsureSuccessStatusCode();

        Console.WriteLine("Key deleted successfully!");
    }
}

// Usage
var client = new KVClient("YOUR_ACCESS_TOKEN");
await client.DeleteKeyAsync("my_key");`,

                history: `using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class KVClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8080/api";

    public KVClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-Access-Token", token);
    }

    // Get version history of a key
    public async Task<ListVersionsResponse> GetKeyHistoryAsync(string keyName)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/keys/{keyName}/history");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ListVersionsResponse>(content);
    }
}

// Usage
var client = new KVClient("YOUR_ACCESS_TOKEN");
var history = await client.GetKeyHistoryAsync("my_key");

Console.WriteLine("Version history:");
foreach (var version in history.History)
{
    Console.WriteLine($"v{version.Version}: {version.Value}");
    Console.WriteLine($"  Created: {version.CreatedAt}");
}`
            },

            go: {
                read: `package main

import (
    "encoding/json"
    "fmt"
    "io/ioutil"
    "net/http"
)

type KeyValueResponse struct {
    Value     string \`json:"value"\`
    Version   int    \`json:"version"\`
    LastModified string \`json:"lastModified"\`
}

func main() {
    token := "YOUR_ACCESS_TOKEN"
    keyName := "my_key"

    req, _ := http.NewRequest("GET", "http://localhost:8080/api/keys/"+keyName, nil)
    req.Header.Set("X-Access-Token", token)

    client := &http.Client{}
    resp, err := client.Do(req)
    if err != nil {
        panic(err)
    }
    defer resp.Body.Close()

    body, _ := ioutil.ReadAll(resp.Body)
    var data KeyValueResponse
    json.Unmarshal(body, &data)

    fmt.Println("Value:", data.Value)
    fmt.Println("Version:", data.Version)
}`,

                create: `package main

import (
    "bytes"
    "encoding/json"
    "fmt"
    "io/ioutil"
    "net/http"
)

type CreateKeyRequest struct {
    Key   string \`json:"key"\`
    Value string \`json:"value"\`
}

func main() {
    token := "YOUR_ACCESS_TOKEN"
    keyName := "my_key"
    value := "my_value"

    payload := CreateKeyRequest{Key: keyName, Value: value}
    jsonData, _ := json.Marshal(payload)

    req, _ := http.NewRequest("POST", "http://localhost:8080/api/keys", bytes.NewBuffer(jsonData))
    req.Header.Set("X-Access-Token", token)
    req.Header.Set("Content-Type", "application/json")

    client := &http.Client{}
    resp, err := client.Do(req)
    if err != nil {
        panic(err)
    }
    defer resp.Body.Close()

    body, _ := ioutil.ReadAll(resp.Body)
    var data map[string]interface{}
    json.Unmarshal(body, &data)

    fmt.Println("Key created successfully!")
    fmt.Println("Key:", data["key"])
    fmt.Println("Version:", data["version"])
}`,

                update: `package main

import (
    "bytes"
    "encoding/json"
    "fmt"
    "io/ioutil"
    "net/http"
)

type UpdateKeyRequest struct {
    Value string \`json:"value"\`
}

func main() {
    token := "YOUR_ACCESS_TOKEN"
    keyName := "my_key"
    newValue := "updated_value"

    payload := UpdateKeyRequest{Value: newValue}
    jsonData, _ := json.Marshal(payload)

    req, _ := http.NewRequest("PUT", "http://localhost:8080/api/keys/"+keyName, bytes.NewBuffer(jsonData))
    req.Header.Set("X-Access-Token", token)
    req.Header.Set("Content-Type", "application/json")

    client := &http.Client{}
    resp, err := client.Do(req)
    if err != nil {
        panic(err)
    }
    defer resp.Body.Close()

    body, _ := ioutil.ReadAll(resp.Body)
    var data map[string]interface{}
    json.Unmarshal(body, &data)

    fmt.Println("Key updated successfully!")
    fmt.Println("New version:", data["version"])
}`,

                list: `package main

import (
    "encoding/json"
    "fmt"
    "io/ioutil"
    "net/http"
)

func main() {
    token := "YOUR_ACCESS_TOKEN"

    req, _ := http.NewRequest("GET", "http://localhost:8080/api/keys", nil)
    req.Header.Set("X-Access-Token", token)

    client := &http.Client{}
    resp, err := client.Do(req)
    if err != nil {
        panic(err)
    }
    defer resp.Body.Close()

    body, _ := ioutil.ReadAll(resp.Body)
    var data map[string]interface{}
    json.Unmarshal(body, &data)

    fmt.Println("Keys:")
    keys := data["keys"].([]interface{})
    for _, key := range keys {
        k := key.(map[string]interface{})
        fmt.Printf("- %s (v%v)\n", k["key"], k["version"])
    }
}`,

                delete: `package main

import (
    "fmt"
    "net/http"
)

func main() {
    token := "YOUR_ACCESS_TOKEN"
    keyName := "my_key"

    req, _ := http.NewRequest("DELETE", "http://localhost:8080/api/keys/"+keyName, nil)
    req.Header.Set("X-Access-Token", token)

    client := &http.Client{}
    resp, err := client.Do(req)
    if err != nil {
        panic(err)
    }
    defer resp.Body.Close()

    if resp.StatusCode == 204 {
        fmt.Println("Key deleted successfully!")
    }
}`,

                history: `package main

import (
    "encoding/json"
    "fmt"
    "io/ioutil"
    "net/http"
)

func main() {
    token := "YOUR_ACCESS_TOKEN"
    keyName := "my_key"

    req, _ := http.NewRequest("GET", "http://localhost:8080/api/keys/"+keyName+"/history", nil)
    req.Header.Set("X-Access-Token", token)

    client := &http.Client{}
    resp, err := client.Do(req)
    if err != nil {
        panic(err)
    }
    defer resp.Body.Close()

    body, _ := ioutil.ReadAll(resp.Body)
    var data map[string]interface{}
    json.Unmarshal(body, &data)

    fmt.Println("Version history:")
    history := data["history"].([]interface{})
    for _, version := range history {
        v := version.(map[string]interface{})
        fmt.Printf("v%v: %s\n", v["version"], v["value"])
        fmt.Printf("  Created: %s\n", v["createdAt"])
    }
}`
            }
        };
    }

    getCodeSample(language, action) {
        if (this.samples[language] && this.samples[language][action]) {
            return this.samples[language][action];
        }
        return '// Sample not available';
    }

    highlight(code, lang) {
        const escape = s => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

        const KW = {
            javascript: /\b(const|let|var|function|return|if|else|for|while|class|new|await|async|import|export|from|of|in|true|false|null|undefined|this|typeof|throw|try|catch|finally)\b/g,
            python:     /\b(def|class|import|from|return|if|elif|else|for|while|in|not|and|or|True|False|None|with|as|try|except|finally|raise|async|await)\b/g,
            curl:       /\b(curl)\b/g,
            csharp:     /\b(using|class|public|private|protected|static|async|await|return|var|string|int|bool|void|new|if|else|foreach|for|while|try|catch|finally|throw|namespace|true|false|null|Task|Console|HttpClient|JsonSerializer|StringContent|Encoding)\b/g,
            go:         /\b(package|import|func|return|if|else|for|range|var|const|type|struct|interface|map|chan|go|defer|select|case|default|break|continue|true|false|nil|fmt|http|json|ioutil|string|int|error)\b/g,
        };

        const patterns = [];

        if (lang !== 'python' && lang !== 'curl') {
            patterns.push([/\/\*[\s\S]*?\*\//g, 'cm']);
        }
        if (lang === 'python' || lang === 'curl') {
            patterns.push([/#[^\n]*/g, 'cm']);
        } else {
            patterns.push([/\/\/[^\n]*/g, 'cm']);
        }
        if (lang === 'javascript') {
            patterns.push([/`(?:[^`\\]|\\.)*`/g, 'st']);
        }
        patterns.push([/"(?:[^"\\]|\\.)*"|'(?:[^'\\]|\\.)*'/g, 'st']);
        if (lang === 'curl') {
            patterns.push([/--?[a-zA-Z][-a-zA-Z]*/g, 'kw']);
            patterns.push([/\$[A-Z_][A-Z0-9_]*/g, 'vr']);
        }
        patterns.push([/\b\d+\.?\d*\b/g, 'nm']);
        if (KW[lang]) patterns.push([KW[lang], 'kw']);

        const taken = new Uint8Array(code.length);
        const ranges = [];

        for (const [re, cls] of patterns) {
            re.lastIndex = 0;
            let m;
            while ((m = re.exec(code)) !== null) {
                const s = m.index, e = m.index + m[0].length;
                let overlap = false;
                for (let i = s; i < e; i++) {
                    if (taken[i]) { overlap = true; break; }
                }
                if (!overlap) {
                    ranges.push([s, e, cls]);
                    taken.fill(1, s, e);
                }
            }
        }

        ranges.sort((a, b) => a[0] - b[0]);

        let html = '', pos = 0;
        for (const [s, e, cls] of ranges) {
            if (pos < s) html += escape(code.slice(pos, s));
            html += `<span class="hl-${cls}">${escape(code.slice(s, e))}</span>`;
            pos = e;
        }
        if (pos < code.length) html += escape(code.slice(pos));
        return html;
    }

    updateSample(language, action) {
        const code = this.getCodeSample(language, action);
        document.getElementById('code-content').innerHTML = this.highlight(code, language);
        document.getElementById('code-title').textContent = `${this.capitalize(language)} - Read Value`;

        const lineNumsEl = document.getElementById('code-line-numbers');
        if (lineNumsEl) {
            const count = (code.match(/\n/g) || []).length + 1;
            lineNumsEl.innerHTML = Array.from({ length: count }, (_, i) => `<span>${i + 1}</span>`).join('');
        }
    }

    capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
}

// Create global code samples instance
const codeSamples = new CodeSamples();