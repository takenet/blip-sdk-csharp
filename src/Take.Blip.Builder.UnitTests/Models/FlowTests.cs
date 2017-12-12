using System;
using System.ComponentModel.DataAnnotations;
using Shouldly;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class FlowTests
    {
        [Fact]
        public void ValidateSingleStateValidFlowShouldSucceed()
        {
            // Arrange
            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input
                        {
                            Variable = "Any"
                        }
                    }
                }
            };

            // Act
            flow.Validate();
        }

        [Fact]
        public void ValidateWithoutIdShouldFail()
        {
            // Arrange
            var flow = new Flow();

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The flow id is required");
            }
        }

        [Fact]
        public void ValidateWithoutRootStateShouldFail()
        {
            // Arrange
            var flow = new Flow
            {
                Id = "0",
                States = new[]
                {
                    new State
                    {
                        Id = "0"
                    }
                }
            };

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The flow must have one root state");
            }
        }

        [Fact]
        public void ValidateRootStateWithoutInputShouldFail()
        {
            // Arrange
            var flow = new Flow
            {
                Id = "0",
                States = new[]
                {
                    new State
                    {
                        Id = "0",
                        Root = true
                    }

                }
            };

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The root state must expect an input");
            }
        }

        [Fact]
        public void ValidateWithTwoRootStatesShouldFail()
        {
            // Arrange
            var flow = new Flow
            {
                Id = "0",
                States = new[]
                {
                    new State
                    {
                        Id = "0",
                        Root = true,
                        Input = new Input()
                    },
                    new State
                    {
                        Id = "1",
                        Root = true,
                        Input = new Input()
                    }
                }
            };

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The flow must have one root state");
            }
        }

        [Fact]
        public void ValidateWithTwoDuplicateIdStatesShouldFail()
        {
            // Arrange
            var flow = new Flow
            {
                Id = "0",
                States = new[]
                {
                    new State
                    {
                        Id = "0",
                        Root = true,
                        Input = new Input()
                    },
                    new State
                    {
                        Id = "1",
                        Input = new Input()
                    },
                    new State
                    {
                        Id = "1",
                        Input = new Input()
                    }
                }
            };

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The state id '1' is not unique in the flow");
            }
        }

        [Fact]
        public void ValidateFlowWithOneStepsDirectLoopShouldFail()
        {
            // Arrange
            var flow = new Flow
            {
                Id = "0",
                States = new[]
                {
                    new State
                    {
                        Id = "0",
                        Root = true,
                        Input = new Input(),
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "1"
                            }
                        }
                    },
                    new State
                    {
                        Id = "1",
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "1"
                            }
                        }
                    }
                }
            };

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("There is a loop in the flow that do not requires user input");
            }
        }

        [Fact]
        public void ValidateFlowWithTwoStepsDirectLoopShouldFail()
        {
            // Arrange
            var flow = new Flow
            {
                Id = "0",
                States = new[]
                {
                    new State
                    {
                        Id = "0",
                        Root = true,
                        Input = new Input(),
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "1"
                            }
                        }
                    },
                    new State
                    {
                        Id = "1",
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "2",
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "1"
                            }
                        }
                    }
                }
            };

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("There is a loop in the flow that do not requires user input");
            }
        }

        [Fact]
        public void ValidateFlowWithMultipleStepsDirectLoopShouldFail()
        {
            // Arrange
            var flow = new Flow
            {
                Id = "0",
                States = new[]
                {
                    new State
                    {
                        Id = "0",
                        Root = true,
                        Input = new Input(),
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "1"
                            }
                        }
                    },
                    new State
                    {
                        Id = "1",
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "2",
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "3"
                            }
                        }
                    },
                    new State
                    {
                        Id = "3",
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "4"
                            }
                        }
                    },
                    new State
                    {
                        Id = "4",
                        Outputs = new []
                        {
                            new Output
                            {
                                StateId = "2"
                            }
                        }
                    }
                }
            };

            // Act
            try
            {
                flow.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("There is a loop in the flow that do not requires user input");
            }
        }
    }
}
