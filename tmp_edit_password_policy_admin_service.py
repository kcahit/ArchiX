from pathlib import Path

path = Path("src/ArchiX.Library/Runtime/Security/PasswordPolicyAdminService.cs")
text = path.read_text()

sig = "    public async Task UpdateAsync(string json, int applicationId = 1, byte[]? clientRowVersion = null, CancellationToken ct = default)"
first = text.find(sig)
if first == -1:
    raise SystemExit("signature not found")
second = text.find(sig, first + 1)
if second == -1:
    raise SystemExit("second signature not found")

start = text.find("{", second)
if start == -1:
    raise SystemExit("brace not found")

count = 0
end = None
for idx in range(start, len(text)):
    ch = text[idx]
    if ch == '{':
        count += 1
    elif ch == '}':
        count -= 1
        if count == 0:
            end = idx + 1
            break

if end is None:
    raise SystemExit("end not found")

trail_end = end
while trail_end < len(text) and text[trail_end] in '\r\n':
    trail_end += 1

new_text = text[:second] + text[trail_end:]

stub = "    public Task UpdateAsync(string json, int applicationId = 1, CancellationToken ct = default)\r\n    {\r\n        throw new NotImplementedException();\r\n    }\r\n"
if stub in new_text:
    new_text = new_text.replace(stub, "", 1)

path.write_text(new_text)
