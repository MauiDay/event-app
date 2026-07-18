using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiDay.App.Services;
using MauiDay.Core.Configuration;

namespace MauiDay.App.ViewModels;

public sealed partial class CodeOfConductViewModel(
    IAppDataService dataService,
    IExternalLauncher launcher) : BaseViewModel
{
    private Uri? _authoritativeUrl;

    [ObservableProperty]
    private IReadOnlyList<OrganizerConfiguration> _organizers = [];

    public IReadOnlyList<CodeOfConductSection> Sections { get; } =
    [
        new(
            "Our pledge",
            "We as members, contributors, and leaders pledge to make participation in our community a harassment-free experience for everyone, regardless of age, body size, visible or invisible disability, ethnicity, sex characteristics, gender identity and expression, level of experience, education, socio-economic status, nationality, personal appearance, race, religion, or sexual identity and orientation.\n\nWe pledge to act and interact in ways that contribute to an open, welcoming, diverse, inclusive, and healthy community."),
        new(
            "Our standards",
            "Examples of behavior that contributes to a positive environment for our community include:\n\n- Demonstrating empathy and kindness toward other people\n- Being respectful of differing opinions, viewpoints, and experiences\n- Giving and gracefully accepting constructive feedback\n- Accepting responsibility and apologizing to those affected by our mistakes, and learning from the experience\n- Focusing on what is best not just for us as individuals, but for the overall community\n\nExamples of unacceptable behavior include:\n\n- The use of sexualized language or imagery, and sexual attention or advances of any kind\n- Trolling, insulting or derogatory comments, and personal or political attacks\n- Public or private harassment\n- Publishing others' private information, such as a physical or email address, without their explicit permission\n- Other conduct which could reasonably be considered inappropriate in a professional setting"),
        new(
            "Recruiters",
            "Recruiters are welcome to attend MAUI Day, but please be aware that we will be monitoring the event for any unsolicited recruitment activity and reserve the right to request they leave. Recruiters are expected to make meaningful connections with other attendees, contribute to the community during the event, and follow up afterwards."),
        new(
            "Enforcement responsibilities",
            "Community leaders are responsible for clarifying and enforcing our standards of acceptable behavior and will take appropriate and fair corrective action in response to any behavior that they deem inappropriate, threatening, offensive, or harmful.\n\nCommunity leaders have the right and responsibility to remove, edit, or reject comments, commits, code, wiki edits, issues, and other contributions that are not aligned to this Code of Conduct, and will communicate reasons for moderation decisions when appropriate."),
        new(
            "Scope",
            "This Code of Conduct applies within all community spaces, and also applies when an individual is officially representing the community in public spaces. Examples of representing our community include using an official email address, posting via an official social media account, or acting as an appointed representative at an online or offline event."),
        new(
            "Enforcement",
            "Instances of abusive, harassing, or otherwise unacceptable behavior may be reported to the community leaders responsible for enforcement using the contacts above. At the start of the event, the organising team will be highlighted to the attendees. You can approach any of the team members if you feel uncomfortable or if you experience any issues.\n\nAll complaints will be reviewed and investigated promptly and fairly. If required, a member of the organising team will escort you from the venue, ensure that you have a safe ride home, and contact the police.\n\nAll community leaders are obligated to respect the privacy and security of the reporter of any incident."),
        new(
            "Enforcement guidelines",
            "1. Correction\n\nCommunity Impact: Use of inappropriate language or other behavior deemed unprofessional or unwelcome in the community.\n\nConsequence: A private, written warning from community leaders, providing clarity around the nature of the violation and an explanation of why the behavior was inappropriate. A public apology may be requested.\n\n2. Warning\n\nCommunity Impact: A violation through a single incident or series of actions.\n\nConsequence: A warning with consequences for continued behavior. No interaction with the people involved, including unsolicited interaction with those enforcing the Code of Conduct, for a specified period of time. This includes avoiding interactions in community spaces as well as external channels like social media. Violating these terms may lead to a temporary or permanent ban.\n\n3. Temporary Ban\n\nCommunity Impact: A serious violation of community standards, including sustained inappropriate behavior.\n\nConsequence: A temporary ban from any sort of interaction or public communication with the community for a specified period of time. No public or private interaction with the people involved, including unsolicited interaction with those enforcing the Code of Conduct, is allowed during this period. Violating these terms may lead to a permanent ban.\n\n4. Permanent Ban\n\nCommunity Impact: Demonstrating a pattern of violation of community standards, including sustained inappropriate behavior, harassment of an individual, or aggression toward or disparagement of classes of individuals.\n\nConsequence: A permanent ban from any sort of public interaction within the community."),
        new(
            "Attribution",
            "This Code of Conduct is adapted from the Contributor Covenant, version 2.0, available at https://www.contributor-covenant.org/version/2/0/code_of_conduct.html.\n\nCommunity Impact Guidelines were inspired by Mozilla's code of conduct enforcement ladder.\n\nFor answers to common questions about this code of conduct, see the FAQ at https://www.contributor-covenant.org/faq. Translations are available at https://www.contributor-covenant.org/translations."),
    ];

    public async Task LoadAsync()
    {
        var snapshot = await GetSnapshotAsync(dataService);
        if (snapshot is null)
        {
            return;
        }

        Organizers = snapshot.Event.Organizers;
        _authoritativeUrl = snapshot.Event.Links.CodeOfConduct;
        StatusMessage = "The authoritative web version is always available below.";
    }

    [RelayCommand]
    private Task EmailOrganizerAsync(OrganizerConfiguration? organizer) =>
        organizer is null
            ? Task.CompletedTask
            : RunExternalActionAsync(
                () => launcher.ComposeEmailAsync(
                    organizer.Email,
                    "MAUI Day Code of Conduct"));

    [RelayCommand]
    private Task OpenAuthoritativeVersionAsync() =>
        _authoritativeUrl is null
            ? Task.CompletedTask
            : RunExternalActionAsync(() => launcher.OpenBrowserAsync(_authoritativeUrl));
}

public sealed record CodeOfConductSection(string Heading, string Body);
