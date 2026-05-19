namespace App.Domain;

public enum TrainingCertificationStatus
{
    Pending,      // user submitted, waiting admin review
    Approved,     // admin verified
    Rejected,     // admin rejected
    Revoked,      // previously approved, later revoked
    Expired       // time-based expiry reached
}