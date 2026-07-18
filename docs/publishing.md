# Publishing the MAUI Day companion app

This repo ships two **release / publish** GitHub Actions workflows that build **signed**
artifacts and push them to the app stores:

| Workflow | File | Runner | Output | Destination |
| --- | --- | --- | --- | --- |
| Publish iOS | [`.github/workflows/publish-ios.yml`](../.github/workflows/publish-ios.yml) | `macos-15` | signed `.ipa` | App Store Connect / TestFlight |
| Publish Android | [`.github/workflows/publish-android.yml`](../.github/workflows/publish-android.yml) | `ubuntu-latest` | signed `.aab` | Google Play |

Both are triggered:

- **Manually** via `workflow_dispatch` (with inputs to override the version / build number, and — for Android — the Play track).
- Automatically when a **GitHub Release is published**, or when a **`v*` tag** is pushed.

> A separate workflow (added in a different PR) handles plain build/test CI. These
> two workflows are **only** about signing + store upload.

Nothing account-specific is committed. Every certificate, key, password, identifier and
account value is read from **GitHub Actions secrets** (`${{ secrets.* }}`) or
**variables** (`${{ vars.* }}`). You must create them under
**Settings → Secrets and variables → Actions** before running the workflows.

---

## Required secrets

Create these under **Settings → Secrets and variables → Actions → Secrets**.

### iOS

| Name | Type | What it is | How to obtain |
| --- | --- | --- | --- |
| `APPLE_DISTRIBUTION_CERT_BASE64` | secret | Your **Apple Distribution** signing certificate + private key, exported as a `.p12` and base64-encoded. | In **Keychain Access**, select your *Apple Distribution* certificate **and** its private key → right-click → *Export 2 items…* → save as `dist.p12` (set an export password). Then base64-encode it: `base64 -i dist.p12 \| pbcopy` (macOS) or `base64 -w0 dist.p12` (Linux). Paste the result. |
| `APPLE_CERT_PASSWORD` | secret | The export password you set when creating the `.p12` above. | The password you typed during the Keychain export. |
| `APPLE_PROVISIONING_PROFILE_BASE64` | secret | An **App Store** distribution provisioning profile (`.mobileprovision`) for bundle id `net.mauiday.companion`, base64-encoded. | Create/download from [developer.apple.com → Certificates, IDs & Profiles → Profiles](https://developer.apple.com/account/resources/profiles/list) (type *App Store Connect*/*App Store*). Then `base64 -i profile.mobileprovision \| pbcopy` / `base64 -w0 profile.mobileprovision`. |
| `APP_STORE_CONNECT_KEY_ID` | secret | The **Key ID** of an App Store Connect API key. | [App Store Connect → Users and Access → Integrations → App Store Connect API](https://appstoreconnect.apple.com/access/integrations/api) → generate a key with the **App Manager** role. The Key ID is shown in the table (e.g. `ABC123DEFG`). |
| `APP_STORE_CONNECT_ISSUER_ID` | secret | The **Issuer ID** for your App Store Connect API keys. | Same *App Store Connect API* page; the Issuer ID (a GUID) is shown at the top of the Keys list. |
| `APP_STORE_CONNECT_PRIVATE_KEY` | secret | The **contents** of the `.p8` private key file for the API key above. | Downloaded **once** when you generate the API key (`AuthKey_XXXXXXXXXX.p8`). Paste the entire file contents, including the `-----BEGIN PRIVATE KEY-----` / `-----END PRIVATE KEY-----` lines. |

### Android

| Name | Type | What it is | How to obtain |
| --- | --- | --- | --- |
| `ANDROID_KEYSTORE_BASE64` | secret | Your Android upload/signing **keystore** (`.keystore` / `.jks`), base64-encoded. | Create one if you don't have it: `keytool -genkeypair -v -keystore upload.keystore -alias upload -keyalg RSA -keysize 2048 -validity 10000`. Encode: `base64 -i upload.keystore \| pbcopy` / `base64 -w0 upload.keystore`. |
| `ANDROID_KEYSTORE_PASSWORD` | secret | The keystore (store) password. | The password you set with `keytool`. |
| `ANDROID_KEY_ALIAS` | secret | The key alias inside the keystore (e.g. `upload`). | The `-alias` you used with `keytool`. |
| `ANDROID_KEY_PASSWORD` | secret | The password for that specific key alias. | The key password from `keytool` (often the same as the store password). |
| `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` | secret | A Google Cloud **service-account JSON** with access to the Play Developer API. | In [Google Play Console → Setup → API access](https://play.google.com/console/), link a Google Cloud project, create a service account, download its JSON key, then grant that account access to your app (*Users and permissions*) with at least **Release to testing tracks / production** permissions. Paste the full JSON. |

---

## Required variables

Create these under **Settings → Secrets and variables → Actions → Variables**.
Variables are **not** secret — they're identifiers safe to display in logs.

| Name | Type | What it is | How to obtain |
| --- | --- | --- | --- |
| `APPLE_TEAM_ID` | variable | Your 10-character Apple Developer **Team ID**. | [developer.apple.com → Membership](https://developer.apple.com/account/#/membership) → *Team ID* (e.g. `AB12CD34EF`). |
| `ANDROID_PACKAGE_NAME` | variable | The Play **application id / package name**. Set to `net.mauiday.companion`. | Matches `ApplicationId` in `src/MauiDay.App/MauiDay.App.csproj`. |

---

## Running the workflows

### From the Actions tab (manual)

1. Go to **Actions** → choose **Publish iOS** or **Publish Android**.
2. Click **Run workflow**.
3. Optionally set:
   - **display_version** — overrides `ApplicationDisplayVersion` (marketing version).
   - **build_number** — overrides `ApplicationVersion` (build number / versionCode). If left empty, the workflow run number is used.
   - **play_track** (Android only) — `internal`, `alpha`, `beta` (default), or `production`.

### From a release / tag

- Publish a **GitHub Release**, or push a tag like `v1.1`. Both workflows run
  automatically. When triggered by a `v*` tag, the tag (minus the leading `v`) is
  used as the marketing version and the run number becomes the build number.

---

## Notes / decisions to confirm

- **iOS target: TestFlight vs full App Store.** The iOS workflow uploads the build to
  App Store Connect via `xcrun altool`. That makes the build available in **TestFlight**
  immediately; promoting it to a public App Store release is still a manual step in App
  Store Connect (submit for review). This is the safer default — change it if you want
  automated store submission.
- **Android default track: `beta`.** The Android workflow defaults to the `beta`
  (Open testing) track, so builds go to external testers with the opt-in link.
  Override per-run via the `play_track` input. The **first** upload of a brand-new
  app usually has to be done manually in the Play Console before the API will accept
  automated uploads.
- **Provisioning profile.** The iOS build uses the profile's *name* for
  `-p:CodesignProvision`. Make sure the profile you encode matches the
  `net.mauiday.companion` bundle id and your distribution certificate.
- **Signing identity.** The workflow auto-detects the imported distribution certificate's
  common name for `-p:CodesignKey`. If you maintain multiple certificates in the keychain,
  you may want to pin this explicitly.
