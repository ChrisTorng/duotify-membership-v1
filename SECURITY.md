# 安全性最佳實踐指南

## 🔐 敏感資訊管理

### 原則
- ❌ **絕對不要** 將真實的密碼、API Key、Token 提交到 Git
- ✅ **務必** 使用環境變數或密鑰管理服務
- ✅ **定期** 輪換所有憑證和密鑰

### 開發環境設定

#### 方法 1: .NET User Secrets (推薦)
```bash
cd src/Duotify.Membership.Api
dotnet user-secrets init
dotnet user-secrets set "SendGrid:ApiKey" "your-real-api-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

#### 方法 2: 環境變數
```bash
# Linux/macOS
export SENDGRID_API_KEY="your-real-api-key"

# Windows PowerShell
$env:SENDGRID_API_KEY="your-real-api-key"
```

#### 方法 3: .env 檔案 (記得加入 .gitignore)
```bash
cp .env.example .env
# 編輯 .env 填入真實值
```

### 生產環境設定

#### Azure Key Vault (推薦)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

#### AWS Secrets Manager
```csharp
builder.Configuration.AddSecretsManager();
```

### 檢查清單

- [ ] `.gitignore` 已包含所有敏感檔案模式
- [ ] 所有範例文件使用佔位符 (如 `YourStrong@Password`)
- [ ] 開發團隊了解不得提交真實憑證
- [ ] CI/CD pipeline 使用密鑰管理服務
- [ ] 定期掃描 Git 歷史記錄是否有洩露

### 洩露應對

如果不慎洩露敏感資訊:

1. **立即撤銷** 洩露的憑證 (API Key, Token, 密碼)
2. **產生新的** 憑證並更新所有服務
3. **清理 Git 歷史**:
   ```bash
   # 使用 git-filter-repo (推薦)
   git filter-repo --invert-paths --path 'path/to/secret/file'
   
   # 或使用 BFG Repo-Cleaner
   bfg --delete-files secrets.json
   ```
4. **強制推送** 清理後的歷史記錄
5. **通知團隊** 所有成員重新 clone 儲存庫

### 自動化掃描工具

- **GitGuardian**: 自動偵測 Git 中的密鑰洩露
- **TruffleHog**: 掃描 Git 歷史記錄
- **git-secrets**: 防止提交密鑰到 Git
- **detect-secrets**: Pre-commit hook

### 相關資源

- [OWASP Secret Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)
