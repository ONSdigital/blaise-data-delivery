Feature: As a responsible data owner
	I want to ensure that the correct data is delivered
	So that the data can be used correctly in analytics

#Instrument / questionnaire name is provided

#Scenario: Instrument is not available
#	Given questionnaire 'niktest7836498632' is not available
#	When the data delivery service processes the questionnaire 'niktest7836498632'
#	Then the service reports that the questionnaire is not available

Scenario: There no cases in the instrument to be delivered
	Given there are no cases in the questionnaire 'OPN2101A'
	When the data delivery service processes the questionnaire 'OPN2101A'
	Then no cases are delivered for 'OPN2101A'
#
Scenario: There are cases in the instrument to be delivered
	Given there are '10' cases in the questionnaire 'OPN2101A'
	When the data delivery service processes the questionnaire 'OPN2101A'
	Then all the cases are delivered for 'OPN2101A'
	And No other questionnaires are delivered
#
#Scenario: There both completed and partially complete cases in the instrument to be delivered
#	Given there are are x completed cases in the questionnaire 'y'
#	And there are x partially completed cases in questionnaire 'y'
#	When the data delivery service processes the questionnaire 'y'
#	Then the x completed cases are delivered
#	And the x partially completed cases are delivered 
#
##Instrument / questionnaire name is NOT provided - process all instruments
#
#Scenario: There are no instruments installed
#	Given there are no questionnaires are not available
#	When the data delivery service attempts to processes all questionnaires
#	Then the service reports that no questionnaires are available
#
Scenario: We have two instruments installed and both have cases
	Given there are '10' cases in the questionnaire 'OPN2101A'
	And there are '10' cases in the questionnaire 'OPN2004A'
	When the data delivery service processes all questionnaires
	Then all the cases are delivered for 'OPN2101A' 
	And all the cases are delivered for 'OPN2004A'
	And No other questionnaires are delivered
#
#Scenario: We have two instruments installed but only one has cases
#	Given there are are cases in the questionnaire 'y'
#	And there are no cases in the questionnaire 'z'
#	When the data delivery service processes all questionnaires
#	Then all the cases are delivered for questionnaire 'y' 
#	And no cases for are delivered questionnaire 'z' 
