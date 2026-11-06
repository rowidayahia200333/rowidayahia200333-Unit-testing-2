# TESTING_NOTES.md

1. Total tests, passing, failing:
   Total tests: 21, Passing: 21, Failing: 0.

2. Line coverage %:    96.6%

3. Where Moq, AutoFixture, and Bogus were used:
    moq: Used for mocking dependencies in unit tests and mock external service and verify method calls
    AutoFixture: Used for generating test data and creating instances of classes with random values for unit tests وبتكون مهمة اكتر للحجات المعقدة دي زي بناء ال object بالكامل اوتوماتيك وبيكون الداتا بتاعتها مش حقيقة مش بتحاكي اسماء حقيقة يعني وكده
    Bogus: Used for generating realistic fake data for unit tests, such as names, addresses, and other types of data وبتسخدم لتحاكي معلومات بسيطة زي دي وحقيقية اكتر 

4. Look at Examples B and C. Is it fair that a customer who pays 999.99 pays more than one who buys for
1,000?
not fair هو اصغر في السعر بحاجة تكاد تكون معدومة بسيطة جدا المفروض في حاجة تكون بتخلييهم متساووين في الخصم مثلا 

5. Any bug you think you found:
   None i think but there is no created at in receipt for example 
  