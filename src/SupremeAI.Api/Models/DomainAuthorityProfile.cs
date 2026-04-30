namespace SupremeAI.Api.Models;

/// <summary>
/// Authority profile for a task domain.
///
/// Formalises how SupremeAI judges acceptability per domain so that
/// recommendations are defensible and auditable.  Each profile captures:
///
///  - <see cref="AcceptedSourceTypes"/>  — guidance on where to verify claims
///    (not assertions of factual truth — see property documentation).
///  - <see cref="HallucinationTolerance"/> — risk sensitivity (low → stricter
///    confidence thresholds and stronger caveats).
///  - <see cref="CreativityTolerance"/>  — latitude for non-literal interpretation.
///  - <see cref="EvidenceExpectations"/> — plain-language statement of the
///    evidence standard for the domain.
///
/// Profiles are loaded from <c>domain_profiles.json</c> at startup by
/// <see cref="Services.DomainProfileRegistry"/> and referenced by the
/// T-101 Judgment Output Contract when generating confidence, reasons,
/// and caveats.
///
/// <b>Design intent:</b> domain profiles are deterministic governance artefacts.
/// They do not assert objective truth, model capabilities, or source reliability.
/// They exist to make SupremeAI's decision criteria inspectable and auditable.
/// </summary>
public sealed class DomainAuthorityProfile
{
    /// <summary>
    /// Canonical domain key used by <see cref="Services.JudgmentEngine.InferDomain"/>
    /// (e.g. "code", "research", "creative").
    ///
    /// Prompts that do not match any specific domain are assigned the
    /// <c>"general"</c> key, which applies medium-tolerance thresholds
    /// and does not assert any domain-specific evidence requirements.
    /// </summary>
    public string Domain { get; set; } = "";

    /// <summary>Human-readable display name for this domain.</summary>
    public string DisplayName { get; set; } = "";

    /// <summary>Brief description of the domain's scope and characteristics.</summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Source types that provide a reasonable basis for verifying claims in
    /// this domain (e.g. "peer-reviewed publications", "official documentation").
    ///
    /// <b>Important:</b> these are guidance for defensible verification — they
    /// indicate <em>where to look</em> when cross-checking AI output, not that
    /// any source is guaranteed correct.  SupremeAI does not assert the factual
    /// accuracy of any source; human judgement remains mandatory.
    ///
    /// Used to enrich caveats for low-tolerance domains.
    /// </summary>
    public List<string> AcceptedSourceTypes { get; set; } = [];

    /// <summary>
    /// Acceptable level of hallucination risk for this domain:
    /// <c>"low"</c> (factual/technical), <c>"medium"</c> (general),
    /// or <c>"high"</c> (creative).
    ///
    /// Lower tolerance triggers stricter confidence thresholds and
    /// stronger source-verification caveats in the T-101 output.
    /// </summary>
    public string HallucinationTolerance { get; set; } = "medium";

    /// <summary>
    /// Acceptable level of creative latitude for this domain:
    /// <c>"low"</c> (precise/technical), <c>"medium"</c> (general),
    /// or <c>"high"</c> (creative/marketing).
    ///
    /// Higher tolerance shifts reasons toward coherence and originality
    /// rather than strict factual precision.
    /// </summary>
    public string CreativityTolerance { get; set; } = "medium";

    /// <summary>
    /// Plain-language statement of the evidence expected in a high-quality
    /// response for this domain — what constitutes acceptable support for claims.
    /// </summary>
    public string EvidenceExpectations { get; set; } = "";

    /// <summary>
    /// Semantic version of this profile definition (e.g. <c>"1.0"</c>).
    ///
    /// Judgment records can reference this version so that auditors can
    /// determine which authority profile was active at decision time —
    /// important when profiles are updated and historical comparisons are needed.
    /// </summary>
    public string Version { get; set; } = "1.0";
}

/// <summary>Response body for <c>GET /supreme/domains</c>.</summary>
public sealed class DomainProfilesResponse
{
    /// <summary>All registered domain authority profiles.</summary>
    public List<DomainAuthorityProfile> Profiles { get; set; } = [];

    /// <summary>Total number of profiles.</summary>
    public int Total { get; set; }
}
